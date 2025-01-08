# agent/llm_agent.py
from langchain_openai import ChatOpenAI
from langchain_core.messages import HumanMessage, AIMessage, SystemMessage, ToolMessage
from langchain.globals import set_debug
from langchain_core.output_parsers import JsonOutputParser
from langchain.tools import tool
from typing import Annotated, Any, Dict, List
from dataclasses import dataclass
from langgraph.graph import StateGraph, START, END 
from langgraph.graph.message import add_messages
from typing import TypedDict
from llm.utils.callbacks import CustomCallbackHandler
from llm.tools.tools import getDevicesUserAngle, getDevicesInSights, operateDevice, getDevices, sortDevices




llm = ChatOpenAI(model="gpt-4o", temperature=0.0)





@tool
def extractDevice(
    devices: Annotated[
        List[Dict[str, Any]], 
        """
        A list of devices you want to extract from.
        """
    ],
    command : Annotated[
        str, 
        "The user command specifying which devices to select."
    ]
    ) -> List[Dict[str, Any]]:
    """
    This function extracts the relevant devices based on the user command.
    You may need this if the commands requres comprehension of devices layout or comples spatial relationships.
    """
    
    print() 
    print("=====================[TOOL] extractDevice=====================")
    system_msg = SystemMessage(content=f"""
You are a **Smart Home Voice Command Assistant** with spatial awareness. Your task is to:
1. Analyze a JSON array of device information to understand their spatial arrangement.
2. Interpret user commands referencing spatial contexts like "middle," "right side," or "closest."
3. Select appropriate devices based on the user's command and return them as a JSON array in the same structure as the input.

---

### **JSON Data Structure**
Input and output JSON data includes:
- **id**: Unique identifier for each device.
- **name**: Human-readable device name.
- **position (x, y, z)**: Device coordinates in a 3D space.
- **distance_from_user**: Euclidean distance from the user.
- **angle**: Angular position relative to the user.

---

### **Steps**
1. **Analyze Device Array**:
   - Group or sort devices by coordinates, distance, or angle to identify patterns.
2. **Interpret Commands**:
   - Understand spatial terms like "middle," "closest," or "furthest."
   - Apply filtering based on user instructions.
3. **Return Selected Devices**:
   - Output the selected devices as a JSON array matching the input structure.

---

### **Examples**
1. **Command**: "Select the two closest devices."
   - get devices with getAllDevices with order "proximity", and then pick two first element in the returned array is the two closest devices.

2. **Command**: "Select the middle two devices."
   - Identify devices in the middle row based on `z` coordinates.

3. **Command**: "Select the rightmost device in the back."
   - Choose the device with the largest `x` and `z` values.

---

### **Key Notes**
- Always analyze the array as a whole to determine spatial relationships.
- Ensure the output matches the input format exactly.
""")
    

    human_msg = HumanMessage(content=f"""

### **User Command**: {command}
### **Device Array**: {devices}

""")

    msgs = [
        system_msg,
        human_msg
    ]


    llm_res = llm.invoke(msgs)

    return_res = JsonOutputParser().invoke(llm_res)




    print("===================================================")
    print()

    return return_res
    
    



@dataclass
class State(TypedDict):
    messages: List

llm_with_tools = llm.bind_tools([getDevices, getDevicesUserAngle, getDevicesInSights,  operateDevice ])

tool_map = {
    "getDevices" : getDevices,
    "getDevicesUserAngle": getDevicesUserAngle,
    "getDevicesInSights": getDevicesInSights,
    "operateDevice": operateDevice,
}

def llm_agent(state: State) -> State:
    print()
    print(">>>>>>>>>>>>>>>>[NODE] llm_agent <<<<<<<<<<<<<<<<")


    llm_res = llm_with_tools.invoke(state["messages"])
    state["messages"].append(llm_res)
    print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>")
    print()
    return state  # 非同期呼び出し

def tool_node(state: State) -> State:
    print()
    print(">>>>>>>>>>>>>>>>[NODE] tool_node <<<<<<<<<<<<<<<<")
    last_state = state["messages"][-1]

    # 全ての tool_calls を処理
    for tool_call in last_state.tool_calls:
        tool_function = tool_map.get(tool_call["name"])

        if tool_function:
            print("EXECUTING FUNCTION: ", tool_call["name"])
            print("ARGS: ", tool_call["args"])
            
            # 非同期呼び出しで関数を実行
            tool_output = tool_function.invoke(tool_call["args"])
            print("OUTPUT: ", tool_output)

            # 対応する ToolMessage を生成
            state["messages"].append(ToolMessage(content=tool_output, tool_call_id=tool_call["id"]))
        else:
            print(f"未定義のツールが呼び出されました: {tool_call['name']}")
    
    print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>")
    print()
    return state

def router(state):
    print()
    print(">>>>>>>>>>>>>>>>[NODE] router <<<<<<<<<<<<<<<<")
    last_message = state["messages"][-1]
    if hasattr(last_message, 'tool_calls') and last_message.tool_calls:

        print(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>")
        print()
        return "tool_node"
    else:
        return END

graph_builder = StateGraph(State)

graph_builder.add_node("llm_agent", llm_agent)
graph_builder.add_node("tool_node", tool_node)

graph_builder.add_edge(START, "llm_agent")
graph_builder.add_conditional_edges("llm_agent", router)
graph_builder.add_edge("tool_node", "llm_agent")


runner = graph_builder.compile()

