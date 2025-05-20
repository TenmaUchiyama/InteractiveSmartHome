from langchain_openai import ChatOpenAI
from enum import Enum
from langchain.tools import StructuredTool
from langchain_core.messages import ToolMessage
from pydantic import BaseModel, Field
from langchain_core.output_parsers import JsonOutputParser
from typing import Literal
from langchain_core.messages import SystemMessage
from sr_app_types.node_types import NODE
from agents.device_filter_agent.filter_tool import getDeviceInFov, getDeviceInDirection, getDevices, getDeviceAroundFurniture
import os

from utils.callbacks import CustomCallbackHandler
from sr_app_types.no_tool_agent_types import State


from pydantic import BaseModel
from typing import Any






script_dir = os.path.dirname(os.path.abspath(__file__))  # スクリプトのあるディレクトリ
file_path = os.path.join(script_dir, "prompts", "no_tool_filter_msg.txt")

with open(file_path, "r", encoding="utf-8") as f:
    filter_message = SystemMessage(f.read())

class FILTER_TOOL(Enum): 
    FOV = "fov" 
    DIR = "direction"
    ALL = "all"
    AROUND_FURNITURE = "around_furniture"
    


callback = CustomCallbackHandler("logs/filter_logs.md")
filter_agent = ChatOpenAI(model="gpt-4o" , temperature= 0.0, callbacks=[callback])



# ツールバインド（全フィルター対応）
# filter_agent_node = llm.bind_tools(
#     [getDeviceInFov, getDeviceInDirection, getDevices, getDeviceAroundFurniture],
#     strict=True
# )

# ツール名と関数の対応マップ
filter_tool_map = {
    FILTER_TOOL.FOV.value: getDeviceInFov, 
    FILTER_TOOL.DIR.value: getDeviceInDirection, 
    FILTER_TOOL.ALL.value: getDevices,
    FILTER_TOOL.AROUND_FURNITURE.value: getDeviceAroundFurniture
}





class FilterOutputData(BaseModel):
    filter_type: str
    params: Any

parser = JsonOutputParser(pydantic_object=FilterOutputData)



def filter_preprocess(state : State):
    user_prompt = state.user_prompt
    input_msg = [
        filter_message, 
        user_prompt
    ]

    state.filterAgent.input_prompt = input_msg
    return state



def filter_agent_node(state : State):
    print("=========[FILTER AGENT NODE]=========")
    res = filter_agent.invoke(state.filterAgent.input_prompt)
    output = parser.invoke(res.content)
    print("=========[FILTER AGENT NODE OUTPUT]=========")
    print(output)
    state.filterAgent.output = output
    return state