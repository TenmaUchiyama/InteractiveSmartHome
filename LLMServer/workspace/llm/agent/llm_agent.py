# agent/llm_agent.py
from langchain_openai import ChatOpenAI
from langchain_core.messages import HumanMessage, AIMessage, SystemMessage, ToolMessage
from langchain.globals import set_debug
from llm.callbacks import CustomCallbackHandler
from llm.tools.tools import getDevicesUserAngle, getDevicesInSights, operateDevice, getDevices
from typing import Dict, List
from dataclasses import dataclass
from langgraph.graph import StateGraph, START, END 
from langgraph.graph.message import add_messages
from typing import TypedDict





llm = ChatOpenAI(model="gpt-4o-mini", callbacks=[CustomCallbackHandler()], verbose=True)

@dataclass
class State(TypedDict):
    messages: List

llm_with_tools = llm.bind_tools([getDevices, getDevicesUserAngle, getDevicesInSights, operateDevice])

tool_map = {
    "getDevices" : getDevices,
    "getDevicesUserAngle": getDevicesUserAngle,
    "getDevicesInSights": getDevicesInSights,
    "operateDevice": operateDevice
}

def llm_agent(state: State) -> State:
    llm_res = llm_with_tools.invoke(state["messages"])
    state["messages"].append(llm_res)
    return state  # 非同期呼び出し

def tool_node(state: State) -> State:
    last_state = state["messages"][-1]
    tool_call = last_state.tool_calls[0]
    tool_function = tool_map.get(tool_call["name"])
    
    if tool_function:
        tool_output = tool_function.invoke(tool_call["args"])  # 非同期呼び出し
        state["messages"].append(ToolMessage(content=tool_output, tool_call_id=tool_call["id"]))
    else:
        print(f"未定義のツールが呼び出されました: {tool_call['name']}")
    
    return state

def router(state):
    last_message = state["messages"][-1]
    if hasattr(last_message, 'tool_calls') and last_message.tool_calls:
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

