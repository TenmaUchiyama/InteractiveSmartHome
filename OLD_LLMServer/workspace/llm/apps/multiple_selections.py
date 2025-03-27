from langchain_openai import ChatOpenAI
from langchain_core.messages import HumanMessage ,SystemMessage, ToolMessage
from langgraph.graph import StateGraph, END 
from langgraph.graph.message import add_messages
from langchain.callbacks.base import BaseCallbackHandler
from typing import Annotated
from typing_extensions import TypedDict 
from llm.tools.tools import operateDevice




DB_SERVER_URL = "http://localhost:4049"


class CustomCallbackHandler(BaseCallbackHandler):
    def on_llm_start(self, serialized, prompts, **kwargs):
        print("\n--- 送信するテキスト ---")
        for i, prompt in enumerate(prompts):
            print(f"Prompt {i + 1}: {prompt}")
        print("\n")

    def on_llm_end(self, response, **kwargs):
        print("\n--- LLMからのレスポンス ---")
        print(response)
        print("\n")



llm = ChatOpenAI(model="gpt-4o-mini", callbacks=[CustomCallbackHandler()], verbose="True")

system_msg = SystemMessage(content="""
System:
You are a smart home assistant that processes user commands to control IoT devices. Based on the user's `user_message` and the provided `devices` (an array of device objects), determine the appropriate actions and execute them using the `operateDevice` function. Provide feedback to the user in the same language as their input, summarizing what was done.

---

Instructions:
1. **Analyze the Inputs**:
   - `user_message`: The user's command (e.g., "Turn on the light," "Set brightness to 50%", "Make it red").
   - `devices`: An array of device objects, each with the following fields:
     - `device_id`: Unique ID of the device.
     - `device_type`: Type of the device (e.g., "light", "curtain").
     - `device_name`: Name of the device.
     - `description`: Text description explaining the device's capabilities and usage.
     - `mqtt_topic`: MQTT topic used for sending control messages to the device.
     - `device_position`: 3D position of the device in the room.

2. **Determine the Actions**:
   - Parse the `user_message` to identify the user's intent.
   - For each device in the `devices` array, determine whether it matches the command in the `user_message`. Use the `device_name`, `device_type`, and `description` fields to infer the appropriate action.
   - For lights:
     - Turn ON/OFF based on keywords like "on", "off".
     - Adjust `intensity` based on percentage-like words (e.g., "50%").
     - Change `color` based on colors or RGB values (e.g., "red", "255,0,0").
   - For curtains:
     - Interpret "open" or "Open" as `intensity: 0`.
     - Interpret "close" or "Close" as `intensity: 100`.
   - If no specific device is mentioned in the `user_message`, apply the action to all compatible devices.

3. **Execute the Actions**:
   - For each matched device, use the determined action to execute `operateDevice` with the required parameters.
   - If no devices match the user's intent, provide a message explaining that no actions were taken.

4. **Provide Feedback**:
   - Respond to the user with a message summarizing what was done for each affected device.
   - Ensure the message is in the same language as the `user_message`.

--- Example Data ---

user_message:
"Turn on the lights and open the curtains."

devices:
[
    {
        "device_id": "123",
        "device_type": "light",
        "device_name": "Living Room Light",
        "description": "A dimmable RGB light in the living room.",
        "mqtt_topic": "home/livingroom/light",
        "device_position": {"x": 1, "y": 2, "z": 3}
    },
    {
        "device_id": "456",
        "device_type": "curtain",
        "device_name": "Bedroom Curtain",
        "description": "An automatic curtain system in the bedroom.",
        "mqtt_topic": "home/bedroom/curtain",
        "device_position": {"x": 4, "y": 5, "z": 6}
    }
]

---

Example Feedback:
- "Turned on the Living Room Light."
- "Opened the Bedroom Curtain."
""")








tool_map = {
    "operateDevice" : operateDevice
    
}



llm_with_tool = llm.bind_tools([operateDevice], strict=True)


class State(TypedDict):
    messages : Annotated[list, add_messages]



def function_agent(state : State) -> State:
    msgs = state["messages"]
    
    response = llm_with_tool.invoke(msgs)    
    state["messages"].append(response)
    return state


def tool_node(state:State) -> State: 
    last_msg = state["messages"][-1]
    for tool in last_msg.tool_calls: 
        tool_func = tool_map[tool["name"]]    
        res = tool_func.invoke(tool["args"])
        state["messages"].append(ToolMessage(content=res, tool_call_id=tool["id"]))
        state["devices"] = res
    return state


def router(state : State) -> ["tool_node",END]:
   
    last_message = state["messages"][-1] 

    if last_message.tool_calls:
        return "tool_node"
    else:

        return END
    


g = StateGraph(State)
g.add_node("function_agent", function_agent)
g.add_node("tool_node", tool_node)



g.set_entry_point("function_agent")
g.add_conditional_edges("function_agent", router)
g.add_edge("tool_node", "function_agent")
runner = g.compile()





def invoke_multiple_selection_agent(user_input:str) -> str:
    state = {
        "messages" : [
            system_msg,
            HumanMessage(content=user_input)
        ]
    }

    res = runner.invoke(state)

    return res["messages"][-1].content





