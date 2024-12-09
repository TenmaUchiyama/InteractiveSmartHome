from uuid import UUID
import httpx
from langchain_openai import ChatOpenAI
from langchain_core.messages import HumanMessage, AIMessage, SystemMessage, ToolMessage
from langgraph.graph import StateGraph, START, END 
from langgraph.graph.message import add_messages
from typing import Dict, List, Tuple
from dataclasses import dataclass
from langchain.tools import tool
from typing import Annotated, List
from typing_extensions import TypedDict 
from IPython.display import display, Image
import json
import os 
from langchain.globals import set_debug

from langchain.callbacks.base import BaseCallbackHandler

mr_server_url = "http://localhost:7070"



set_debug(False)



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




callback_handler = CustomCallbackHandler()
llm = ChatOpenAI(model="gpt-4o-mini",callbacks=[callback_handler], verbose="True")


@dataclass
class DeviceData:
    device_id: UUID
    device_name: str
    distance_from_user: float
    device_position: Tuple[float, float, float]



@tool
def getDevicesUserAngle(direction: Annotated[str, """
- direction (str): The direction to search for devices. Possible values are:
      - "Front" (devices in front of the user)
      - "Back" (devices behind the user)
      - "Left" (devices to the left of the user)
      - "Right" (devices to the right of the user)
"""]) -> Dict:
    """
    the function to get the devices based on the user's direction.
    """
    # POSTリクエストのペイロードを作成
    request_body = {
        "function": "direction",
        "args": [direction]
    }
    
    try:
        print(f"Sending POST request to http://localhost:7070/ with body: {request_body}")
        
        response = httpx.post(mr_server_url, json=request_body)
        
        if response.status_code == 200:
            response_data = response.json()
            print(f"Response from server: {response_data}")
            
            if response_data["status"] == "success":
                return response_data
            else:
                return {"status": "error", "message": "Server responded with an error", "details": response_data}
        else:
            return {"status": "error", "message": f"HTTP Error {response.status_code}", "details": response.text}
    except Exception as e:
        return {"status": "error", "message": str(e)}






@tool
def getDevicesInSights(in_sight: Annotated[bool, """
- in_sight (bool): If True, returns devices that are within the user's line of sight.
"""]) -> Dict:
    """
    the function to get the devices based on the user's line of sight.
    """
    # POSTリクエストのペイロードを作成
    request_body = {
        "function": "sight",
        "args": [str(in_sight)]
    }
    
    try:
        print(f"Sending POST request to http://localhost:7070/ with body: {request_body}")
        
        response = httpx.post(mr_server_url, json=request_body)
        
        if response.status_code == 200:
            response_data = response.json()
            print(f"Response from server: {response_data}")
            
            if response_data["status"] == "success":
                return response_data
            else:
                return {"status": "error", "message": "Server responded with an error", "details": response_data}
        else:
            return {"status": "error", "message": f"HTTP Error {response.status_code}", "details": response.text}
    except Exception as e:
        return {"status": "error", "message": str(e)}
    




@tool 
def operateDevice(device_id: Annotated[List[str], "single or multiple device id that need to be operated"]):
    """
    Send the manipulation signal from mqtt-topic associated with the device.
    """
    # device_idをすべてprintする
    print("Device IDs to operated:")
    print("***************************")
    for i, dev_id in enumerate(device_id, start=1):
        print("----------")
        print(f"{i}. {dev_id}")
    print("***************************")

    response = httpx.post(mr_server_url + "/operate", json={"device_id": device_id})
    return "succeed"

llm_with_tools = llm.bind_tools([getDevicesUserAngle,getDevicesInSights , operateDevice])
tool_map = {
    "getDevicesUserAngle" : getDevicesUserAngle,
    "getDevicesInSights" : getDevicesInSights ,
    "operateDevice" : operateDevice
}




class State(TypedDict):
    messages : Annotated[list, add_messages]


def llm_agent(state: State) -> State:
    
    llm_res = llm_with_tools.invoke(state["messages"])

    state["messages"].append(llm_res)
    
    
    return state  # 非同期呼び出し


def tool_node(state: State) -> State:
    last_state = state["messages"][-1]
    tool_function = tool_map[last_state.tool_calls[0]["name"]]
    
    tool_output =  tool_function.invoke(last_state.tool_calls[0]["args"])  # 非同期呼び出し
 
    state["messages"].append(ToolMessage(content=tool_output, tool_call_id=last_state.tool_calls[0]["id"]))

    return state

def router(state: State) -> ["tool_node", END]:
    last_message = state["messages"][-1]
    if last_message.tool_calls:
        return "tool_node"
    else:
        return END
    


graph_builder = StateGraph(State)

graph_builder.add_node("llm_agent",llm_agent)
graph_builder.add_node("tool_node", tool_node)

graph_builder.add_edge(START, "llm_agent")
graph_builder.add_conditional_edges("llm_agent",router ) 

graph_builder.add_edge("tool_node", "llm_agent") 

runner = graph_builder.compile()






def invoke_llm_agent(user_input :str):
    
    state ={"messages": [SystemMessage(content="""
    You are an AI assistant for controlling smart home devices. Based on voice commands from the user, you must identify and control devices considering the user's position, direction, and line of sight. You have access to functions that can retrieve devices based on their spatial relationship to the user.

    Depending on the context of the command, you may need to select a single device or multiple devices to perform the desired action. Your task is to process the user's command and use the available functions to find the appropriate devices, whether it's one or many, and perform the required actions.

    Finally, whey you find the devices or not, find the best function to operate them.
    """)]}



    state["messages"].append(HumanMessage(content=user_input))





    res = runner.invoke(state)

    return res["messages"][-1].content



if __name__ == "__main__":
    response = invoke_llm_agent("Turn on the lights in front of me")

    print("\n--- 最終的なレスポンス ---")
    print(response)
    print("\n")