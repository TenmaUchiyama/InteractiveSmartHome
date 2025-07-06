from langchain_openai import ChatOpenAI
from enum import Enum
from langchain.tools import StructuredTool
from langchain_core.messages import ToolMessage
from pydantic import BaseModel, Field
from langchain_core.output_parsers import JsonOutputParser
from typing import Literal
from langchain_core.messages import SystemMessage, HumanMessage
from sr_app_types.node_types import NODE
from agents.device_filter_agent.no_tool_filter_algorithm import getDeviceInFov, getDeviceInDirection, getDevices, getDeviceAroundFurniture
import os

from utils.callbacks import CustomCallbackHandler
from sr_app_types.no_tool_agent_types import FilterAgentOutput, State


from pydantic import BaseModel
from typing import Any






script_dir = os.path.dirname(os.path.abspath(__file__))  
language = os.getenv("VOICE_LANG", "en").strip() if os.getenv("VOICE_LANG") else "en"
if language == "ja":
    file_path = os.path.join(script_dir, "prompts", "jp_filter_msg.txt")
else:
    file_path = os.path.join(script_dir, "prompts", "no_tool_filter_msg.txt")





with open(file_path, "r", encoding="utf-8") as f:
    filter_message = SystemMessage(f.read())

class FILTER_TOOL(Enum): 
    FOV = "fov" 
    DIR = "direction"
    ALL = "all"
    AROUND_FURNITURE = "around_furniture"
    


callback = CustomCallbackHandler("logs/filter_logs.md")
model = os.getenv("GPT_MODEL")
model = model.strip() if model and model.strip() else "gpt-4o"
filter_agent = ChatOpenAI(model= model, temperature= 0.0, callbacks=[callback])



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


def change_filter_model(model_name):
    global filter_agent
    filter_agent = ChatOpenAI(model=model_name, verbose=True, callbacks=[callback])



class FilterOutputData(BaseModel):
    filter_type: str
    params: Any

parser = JsonOutputParser(pydantic_object=FilterOutputData)



def filter_preprocess(state : State):
    callback.system_start()
    user_prompt = state.user_prompt
    input_msg = [
        filter_message, 
        HumanMessage(content=user_prompt)
    ]

    state.filterAgent.input_prompt = input_msg
    return state


def filter_agent_node(state: State):

    res = filter_agent.invoke(state.filterAgent.input_prompt)
    output_dict = parser.invoke(res.content)
    
    # 🔧 明示的に型変換
    output = FilterAgentOutput(**output_dict)
    print("========[FILTER AGENT NODE]=========")
    print("Filter Type: ", output) 
    print()
    print("Output: ", output.reasoning)
    print()

    state.filterAgent.output_tool_selection = output

    state.filterAgent.metrics = {
        "model_name": callback.model_name,
        "tokens": callback.last_tokens,
        "cost_usd": callback.last_cost,
        "agent_time_elapsed": callback.last_time
    }
    return state



def filter_tool_node(state: State):
    # print(state.filterAgent.output_tool_selection)
    output = state.filterAgent.output_tool_selection
    if output is None:
        return None
    # print(output)
    tool_func = filter_tool_map.get(output.filter_type)

    if tool_func:
        result = tool_func.invoke(input={"params": output.params})
        # print("result: ", result)
        state.filterAgent.devices = result["devices"]
        
        state.filterAgent.metrics["system_time_elapsed"] = callback.system_end()
        return state
    state.filterAgent.metrics["system_time_elapsed"] = callback.system_end()
    return None