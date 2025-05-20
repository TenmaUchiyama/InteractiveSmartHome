from langchain_core.messages import ToolMessage
from langchain_openai import ChatOpenAI
from pydantic import BaseModel
from langchain_core.output_parsers import JsonOutputParser
from langchain_core.messages import HumanMessage, SystemMessage
from typing import Any, List, Dict
import json
import os
from utils.callbacks import CustomCallbackHandler
from sr_app_types.node_types import NODE
from sr_app_types.no_tool_agent_types import State, SpatialInput, SpatialOutput, Device, DeviceState, Color
from agents.device_operator_agent.operator_tool import operateDevice

# プロンプトの読み込み
script_dir = os.path.dirname(os.path.abspath(__file__))
file_path = os.path.join(script_dir, "prompts", "no_tool_spatial_msg.txt")
with open(file_path, "r", encoding="utf-8") as f:
    spatial_message = SystemMessage(f.read())

# コールバック設定と LLM の初期化
callback = CustomCallbackHandler("logs/spatial_logs.md")
spatial_agent = ChatOpenAI(model="gpt-4o", verbose=True, callbacks=[callback])

parser = JsonOutputParser(pydantic_object=SpatialOutput)




def sr_preprocess_node(state: State):
    print("=========[SR PREPROCESS NODE]=========")



    input_data = f"""
           {{ 
            "filter_type": {state.filterAgent.selected_tool["filter_type"]},
            "user_prompt": {state.user_prompt.content},
            "devices":{ state.filterAgent.devices}
            }}
        """

    print(input_data)
    sr_input_msg = HumanMessage(input_data)

    sr_msgs = [
        spatial_message,
        sr_input_msg
    ]
   
    state.spatialAgent.input_prompt = sr_msgs
    return state



def sr_agent_node(state: State):
    print("=========[SR AGENT NODE]=========")
    res = spatial_agent.invoke(state.spatialAgent.input_prompt)
    
    output = parser.invoke(res.content)
    print(output)
    state.spatialAgent.output_data = output
    return state
    

def sr_tool_node(state: State):
    print("=========[SR TOOL NODE]=========")
   
    outputDevice = {"devices": state.spatialAgent.output_data["devices"]}
    print(outputDevice)
    result = operateDevice.invoke(outputDevice)
    print(result)
    return state
    