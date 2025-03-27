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
    

llm = ChatOpenAI(model="gpt-4o" , temperature= 0.0, callbacks=[])



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
    return
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
    debug_log("=========[FILTER] AGENT===========",True)
    last_message = state.filterAgent.messages[-1]
    
    debug_log(last_message)
    if is_tool_message(last_message):
        debug_log("This is a tool message.")
    else:
        debug_log("This is NOT a tool message.")
    agent_res = filter_agent.invoke(state.filterAgent.messages)
    debug_log(agent_res,True)
    state.filterAgent.messages.append(agent_res) 
    return state
    
def filter_tool_node(state):
    try:
        debug_log("=========[FILTER] TOOL ===========", True)

        last_message = state.filterAgent.messages[-1]

        # `tool_calls` の存在確認
        if not hasattr(last_message, 'tool_calls') or not last_message.tool_calls:
            raise ValueError("No tool_calls found in the last message")

        # すべての `tool_calls` を処理
        for t in last_message.tool_calls:
            tool_call_id = t["id"]
            tool_function = filter_tool_map.get(t["name"])

            if tool_function is None:
                raise ValueError(f"Unknown tool function: {t['name']}")

            tool_output = tool_function.invoke(t["args"])
            print("***************TOOL OUTPUT)****************************")
            print(tool_output)
            print("***************TOOL OUTPUT)****************************")

            tool_message = ToolMessage(
                content="Successfully Get the All Device Data. You can safully finish tool calling." if tool_output.get('status') == 'success' else "FAILED",
                tool_call_id=tool_call_id  # すべてのツールに対して適切な `tool_call_id` を渡す
            )

            # デバイスリストを更新
            state.filterAgent.devices.extend(tool_output.get('devices', []))  

            # `ToolMessage` を適切に追加
            state.filterAgent.messages.append(tool_message)

            print("PARAM",tool_output)
            state.filterAgent.tool_parameter = tool_output.get('param', {})

        debug_log("==============")
        return state

    except Exception as e:
        debug_log("Error Occurred: ", e)

        # エラーメッセージをすべての `tool_call_id` に関連付ける
        for t in last_message.tool_calls:
            tool_call_id = t["id"]
            error_message = ToolMessage(
                content=f"Error Occurred: {str(e)}",
                tool_call_id=tool_call_id
            )
            state.filterAgent.messages.append(error_message)

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
    debug_log("=========[FILTER]  POST PROCESS ===========",True)
    last_msg =  state.filterAgent.messages[-1]
    output = parser.invoke(last_msg.content)
    output['params'] = state.filterAgent.tool_parameter
    debug_log(f"フォーマットする: { output}",True)
    state.filterAgent.final_output = output
    return state



