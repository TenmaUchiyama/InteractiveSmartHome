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
from sr_app_types.no_tool_agent_types import PointingState, State,  SpatialOutput
from agents.device_operator_agent.operator_tool import operateDevice

# プロンプトの読み込み
script_dir = os.path.dirname(os.path.abspath(__file__))
file_path = os.path.join(script_dir, "prompts", "no_tool_spatial_msg.txt")
with open(file_path, "r", encoding="utf-8") as f:
    spatial_message = SystemMessage(f.read())

# コールバック設定と LLM の初期化
callback = CustomCallbackHandler("logs/spatial_logs.md")
spatial_agent = ChatOpenAI(model=os.getenv("GPT_MODEL"), verbose=True, callbacks=[callback])

parser = JsonOutputParser(pydantic_object=SpatialOutput)




def sr_preprocess_node(state: State):
    # print("=========[SR PREPROCESS NODE]=========")
    # print("RESULT" ,state.filterAgent)
    callback.system_start()
    input_payload = {
        "filter_type": state.filterAgent.output_tool_selection.filter_type,
        "params": state.filterAgent.output_tool_selection.params,
        "reasoning": state.filterAgent.output_tool_selection.reasoning,
        "user_prompt": state.user_prompt,
        "devices": state.filterAgent.devices
    }
 
    sr_input_msg = HumanMessage(content=json.dumps(input_payload, ensure_ascii=False))

    state.spatialAgent.input_prompt = [spatial_message, sr_input_msg]



    return state


def sr_agent_node(state: State):
    # print("=========[SR AGENT NODE]=========")

    res = spatial_agent.invoke(state.spatialAgent.input_prompt)
    print("Devices", state.filterAgent.devices)
    output = parser.invoke(res.content)
    print("==========[SR AGENT NODE]=========")
   
    print("OUTPUT: ", output["reasoning"])
    print("OUTPUT: ", output["response"])
    state.spatialAgent.output_data = output
    state.spatialAgent.metrics = {
        "model_name": callback.model_name,
        "tokens": callback.last_tokens,
        "cost_usd": callback.last_cost,
        "agent_time_elapsed": callback.last_time
    }
    return state


def sr_tool_node(state: State):
    # print("=========[SR TOOL NODE]=========")
  
    outputDevice = {"devices": state.spatialAgent.output_data["devices"]}
    state.selected_devices = outputDevice["devices"]
    result = operateDevice.invoke(outputDevice)
    state.spatialAgent.metrics["system_time_elapsed"] = callback.system_end()
    
    return state


def system_post_process_node(state: State):
    # print("=========[SYSTEM POST PROCESS NODE]=========")

    filter_metrics = state.filterAgent.metrics or {}
    spatial_metrics = state.spatialAgent.metrics or {}

    # ネストされたトークンを取得
    filter_tokens = filter_metrics.get("tokens", {})
    spatial_tokens = spatial_metrics.get("tokens", {})

    total_prompt_tokens = filter_tokens.get("prompt_tokens", 0) + spatial_tokens.get("prompt_tokens", 0)
    total_completion_tokens = filter_tokens.get("completion_tokens", 0) + spatial_tokens.get("completion_tokens", 0)
    total_tokens = total_prompt_tokens + total_completion_tokens

    total_time = filter_metrics.get("agent_time_elapsed", 0.0) + spatial_metrics.get("agent_time_elapsed", 0.0)
    total_cost = filter_metrics.get("cost_usd", 0.0) + spatial_metrics.get("cost_usd", 0.0)

    total_system_time = filter_metrics.get("system_time_elapsed", 0.0) + spatial_metrics.get("system_time_elapsed", 0.0)

    state.system_metrics = {
        "total_tokens": {
            "total_prompt_tokens": total_prompt_tokens,
            "total_completion_tokens": total_completion_tokens,
            "total_tokens": total_tokens,
        },
        "total_cost_usd": round(total_cost, 6),
        "total_agent_time_elapsed": round(total_time, 4),
        "total_system_time_elapsed": round(total_system_time, 4)
    }

    return state







"""
{
  "filter_type": "fov" | "direction" | "all" | "around_furniture",
  "params": {
    "order": "proximity" | "right" | "high",
    "range": 0.0
  },
  "user_prompt": "<user's natural language instruction>",
  "reasoning": "<reasoning from FilterDeviceAgent>",
  "devices": [
    {
      "id": "device_id",
      "name": "light1",
      "position": { "x": 0.5, "y": 2.0, "z": 3.0 },
      "distance_from_user": 3.1,
      "eye_centrality_score": 14.8
    },
    ...
  ]
}
"""






def pointing_spatial_node(state: PointingState):
    state.callback = callback
    state.callback.system_start()
    


    input_content = {
        "filter_type": "pointer",
        "user_prompt": state.user_prompt,
        "pointed_devices": state.pointed_devices,
        "reasoning": "Selected devices are pointed by user", 
        "devices": state.pointed_devices
    }

    state.input_prompt = [
        spatial_message,
        HumanMessage(content=json.dumps(input_content, ensure_ascii=False))
    ]

    response = spatial_agent.invoke(state.input_prompt)

   

    parsed = parser.parse(response.content)
    state.agent_output = parsed
    state.metrics = {
        "model_name": state.callback.model_name,
        "tokens": state.callback.last_tokens,
        "cost_usd": state.callback.last_cost,
        "agent_time_elapsed": state.callback.last_time
    }
    return state
