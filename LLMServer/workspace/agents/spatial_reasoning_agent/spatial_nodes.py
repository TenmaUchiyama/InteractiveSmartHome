from langchain_core.messages import ToolMessage
from langchain_openai import ChatOpenAI
from pydantic import BaseModel, Field
from langchain_core.output_parsers import JsonOutputParser
from langchain_core.messages import HumanMessage, SystemMessage
from typing import Any, Literal
from agents.device_filter_agent.filter_nodes import FilterOutputData
import os

from utils.callbacks import CustomCallbackHandler

script_dir = os.path.dirname(os.path.abspath(__file__))  # スクリプトのあるディレクトリ
file_path = os.path.join(script_dir, "prompts", "spatial_msg.txt")
with open(file_path, "r", encoding="utf-8") as f:
    spatial_message = SystemMessage(f.read())




llm_spatial = ChatOpenAI(model="gpt-4o", verbose=True, callbacks=[])



class DeviceGroupsOutput(BaseModel):
    device_groups: Any

parser = JsonOutputParser(pydantic_object=DeviceGroupsOutput)




def sr_preprocess_node(state): 
    # print("=======[Spatial Reasoning] PREPROCESS ================")
    
    sr_input_msg = HumanMessage(f"""
    User Input: {state.user_prompt}
    InputFromDeviceFilterAgent: {state.filterAgent.final_output}
    Devices: {state.filterAgent.devices}
    """)

    # print(sr_input_msg)
    
    
    sr_msgs = [
        spatial_message,
        sr_input_msg
    ]
    state.spatialAgent.messages = sr_msgs
    return state


def sr_agent_node(state):
    # print("=========[Spatial Reasoning]  AGENT ===========")
    sr_res = llm_spatial.invoke(state.spatialAgent.messages)
    state.spatialAgent.messages.append(sr_res)
    return state





def sr_postprocess_node(state):
    # print("=======[Spatial Reasoning]  POSTPROCESS ================")
    
    last_msg = state.spatialAgent.messages[-1]
    output = parser.invoke(last_msg.content)
    # print("Output: ", output)
    state.spatialAgent.final_output = output

    
    return state
