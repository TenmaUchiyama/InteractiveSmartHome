from langchain_openai import ChatOpenAI
from enum import Enum
from langchain.tools import StructuredTool
from langchain_core.messages import ToolMessage
from pydantic import BaseModel, Field
from langchain_core.output_parsers import JsonOutputParser
from typing import Literal
from langchain_core.messages import SystemMessage
from sr_app_types.node_types import NODE
from agents.device_filter_agent.filter_tool import getDeviceInFov, getDeviceInDirection, getDevices
import os

from utils.callbacks import CustomCallbackHandler

script_dir = os.path.dirname(os.path.abspath(__file__))  # スクリプトのあるディレクトリ
file_path = os.path.join(script_dir, "prompts", "gp_filter_msg.txt")

with open(file_path, "r", encoding="utf-8") as f:
    filter_message = SystemMessage(f.read())

class FILTER_TOOL(Enum): 
    FOV = "getDeviceInFov" 
    DIR = "getDeviceInDirection"
    ALL = "getDevices"
    


callback = CustomCallbackHandler("logs/filter_logs.md")
llm = ChatOpenAI(model="gpt-4o" , temperature= 0.0, callbacks=[callback])



filter_agent = llm.bind_tools([getDeviceInFov,getDeviceInDirection, getDevices],strict=True) 

filter_tool_map = {
    FILTER_TOOL.FOV.value  : getDeviceInFov, 
    FILTER_TOOL.DIR.value  : getDeviceInDirection, 
    FILTER_TOOL.ALL.value : getDevices
}





class FilterOutputData(BaseModel):
    filter_type: Literal['sight', 'direction', 'object']
    order: Literal['proximity', 'right', 'height']
    devices: list


parser = JsonOutputParser(pydantic_object=FilterOutputData)




def debug_log(msg,flag=True):
 
    if flag: 
        print(msg)
    else:
        print("")

    # print(msg)

def filter_preprocess_node(state): 
    debug_log("=========[FILTER]  PRE PROCESS ===========")
    input_msgs = [
    filter_message, 
    state.user_prompt
    ]


    state.filterAgent.messages = input_msgs 
    return state


def is_tool_message(message):
    return (
        hasattr(message, "tool_calls") and message.tool_calls or
        hasattr(message, "additional_kwargs") and "tool_calls" in message.additional_kwargs
    )

def filter_agent_node(state): 
    debug_log("=========[FILTER] AGENT===========", True)
    last_message = state.filterAgent.messages[-1]

    debug_log(last_message)
    if is_tool_message(last_message):
        debug_log("This is a tool message.")
    else:
        debug_log("This is NOT a tool message.")

    agent_res = filter_agent.invoke(state.filterAgent.messages)
    debug_log(agent_res, True)

    state.filterAgent.messages.append(agent_res)

    # === ログ記録 ===
    state.logger.log(
        node="filter_agent_node",
        node_type="agent",
        input_data=last_message.content if hasattr(last_message, "content") else str(last_message),
        output_data=agent_res.content if hasattr(agent_res, "content") else str(agent_res),
        output_type="tool_call" if getattr(agent_res, "tool_calls", None) else "AIMessage"
    )

    return state

    
def filter_tool_node(state):
    try:
        debug_log("=========[FILTER] TOOL ===========", True)
        last_message = state.filterAgent.messages[-1]

        if not hasattr(last_message, 'tool_calls') or not last_message.tool_calls:
            raise ValueError("No tool_calls found in the last message")

        for t in last_message.tool_calls:
            tool_call_id = t["id"]
            tool_function = filter_tool_map.get(t["name"])
            if tool_function is None:
                raise ValueError(f"Unknown tool function: {t['name']}")

            tool_output = tool_function.invoke(t["args"])
<<<<<<< HEAD
=======
            print("***************TOOL OUTPUT)****************************")
            print(tool_output)
            print("***************TOOL OUTPUT)****************************")
>>>>>>> parent of 1510e9d (new)

            state.logger.log(
                node=t["name"],
                node_type="tool",
                input_data=t["args"],
                output_data=tool_output,
                output_type="ToolMessage"
            )

            tool_message = ToolMessage(
                content=f"Successfully Get the All Device Data. You can safully finish tool calling. {tool_output.get('devices')}" if tool_output.get('status') == 'success' else "FAILED",
                tool_call_id=tool_call_id
            )

            state.filterAgent.devices.extend(tool_output.get('devices', []))
            state.filterAgent.messages.append(tool_message)
<<<<<<< HEAD
=======

            print("PARAM",tool_output)
>>>>>>> parent of 1510e9d (new)
            state.filterAgent.tool_parameter = tool_output.get('param', {})

        return state

    except Exception as e:
        debug_log("Error Occurred: ", e)
        for t in last_message.tool_calls:
            tool_call_id = t["id"]
            error_message = ToolMessage(content=f"Error Occurred: {str(e)}", tool_call_id=tool_call_id)
            state.filterAgent.messages.append(error_message)

        state.logger.log(
            node="FilterToolNode",
            node_type="tool",
            input_data="(unknown due to error)",
            output_data=str(e),
            output_type="ToolMessage"
        )

        return state


def filter_router(state): 
    last_msg  = state.filterAgent.messages[-1]

    if is_tool_message(last_msg):
        
        print("ツールを呼びます")
        return NODE.FILTER_TOOL.value
    else:
        
        print("ツールを呼びません")
        return NODE.FILTER_POSTPROCESS.value

def filter_final_router(state):
    
    if state.filterAgent.final_output['isSpatialReasoningRequired']:
        return NODE.SR_PREPROCESS.value
    else:
        state.spatialAgent.final_output = {
              "selected_groups": [
                  {
                      "group": "",
                      "devices": state.filterAgent.devices,
                      "reasoning" : "No Spatial Reasoning Required"
                  }
              ],
        }
        
        return NODE.OPERATOR_PREPROCESS.value



def filter_postprocess_node(state):
    debug_log("=========[FILTER]  POST PROCESS ===========", True)
    last_msg = state.filterAgent.messages[-1]
    debug_log(f"フォーマットする前: {last_msg.content}", True)

    output = parser.invoke(last_msg.content)
    output['params'] = state.filterAgent.tool_parameter
<<<<<<< HEAD
    debug_log(f"フォーマットした内容: {output}", True)

    state.logger.log(
        node="filter_postprocess_node",
        node_type="postprocess",
        input_data=last_msg.content,
        output_data=output,
        output_type="parsed_json"
    )

=======
    debug_log(f"フォーマットする: { output}",True)
>>>>>>> parent of 1510e9d (new)
    state.filterAgent.final_output = output
    return state




