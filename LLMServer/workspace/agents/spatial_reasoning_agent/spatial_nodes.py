from langchain_core.messages import ToolMessage
from langchain_openai import ChatOpenAI
from pydantic import BaseModel, Field
from langchain_core.output_parsers import JsonOutputParser
from langchain_core.messages import HumanMessage, SystemMessage
from typing import Any
from agents.device_filter_agent.filter_nodes import FilterOutputData
from agents.device_operator_agent.operator_node import operateDevice
import os

from utils.callbacks import CustomCallbackHandler
from sr_app_types.node_types import NODE

# プロンプトの読み込み
script_dir = os.path.dirname(os.path.abspath(__file__))  # スクリプトのあるディレクトリ
file_path = os.path.join(script_dir, "prompts", "spatial_msg.txt")
with open(file_path, "r", encoding="utf-8") as f:
    spatial_message = SystemMessage(f.read())

# コールバック設定と LLM の初期化
callback = CustomCallbackHandler("logs/spatial_logs.md")
llm_sp = ChatOpenAI(model="gpt-4o", verbose=True, callbacks=[callback])
# operateDevice ツールをバインド（operator と同じツールを利用）
llm_spatial = llm_sp.bind_tools(tools=[operateDevice], strict=True)

# operateDevice ツールのマッピング（後述の spatial_tool_node で利用）
tool_map = {
    "operateDevice": operateDevice
}

# 出力の構造体（出力パーサ用）
class DeviceGroupsOutput(BaseModel):
    device_groups: Any

parser = JsonOutputParser(pydantic_object=DeviceGroupsOutput)

def sr_preprocess_node(state): 
    # ======= [Spatial Reasoning] PREPROCESS =======
    print("====[Spatial Reasoning] PREPROCES ====")
    sr_input_msg = HumanMessage(f"""
    User Input: {state.user_prompt}
    InputFromDeviceFilterAgent: {state.filterAgent.final_output}
    Devices: {state.filterAgent.devices}
    """)
    print(sr_input_msg)
    
    sr_msgs = [
        spatial_message,
        sr_input_msg
    ]
    state.spatialAgent.messages = sr_msgs
    return state

def sr_agent_node(state):
    print("====[Spatial Reasoning] AGENT ====")
    last_message = state.spatialAgent.messages[-1]

    sr_res = llm_spatial.invoke(state.spatialAgent.messages)
    state.spatialAgent.messages.append(sr_res)

    # === ログ記録 ===
    state.logger.log(
        node="sr_agent_node",
        node_type="agent",
        input_data=last_message.content if hasattr(last_message, "content") else str(last_message),
        output_data=sr_res.content if hasattr(sr_res, "content") else str(sr_res),
        output_type="tool_call" if getattr(sr_res, "tool_calls", None) else "AIMessage"
    )

    return state


def spatial_tool_node(state):
    print("====[Spatial Reasoning] TOOL NODE ====")
    last_msg = state.spatialAgent.messages[-1]

    if getattr(last_msg, "tool_calls", None):
        for this_tool in last_msg.tool_calls:
            tool_call_id = this_tool['id']
            tool_function = tool_map.get(this_tool['name'])

            if tool_function is None:
                raise ValueError(f"Tool function not found for {this_tool['name']}")

            tool_output = tool_function.invoke(this_tool['args'])

            state.logger.log(
                node=this_tool["name"],
                node_type="tool",
                input_data=this_tool["args"],
                output_data=tool_output,
                output_type="ToolMessage"
            )

            tool_message = ToolMessage(
                tool_call_id=tool_call_id,
                content=tool_output
            )
            state.spatialAgent.messages.append(tool_message)

    return state

def spatial_router(state):
    """
    Spatialエージェント内で、LLMの最終メッセージにツール呼び出しがあるかどうかで、
    次に実行すべきノードを分岐させる（例：ツール呼び出しがあれば spatial_tool_node、なければ後処理）。
    """
    last_msg = state.spatialAgent.messages[-1]
    if getattr(last_msg, "tool_calls", None):
        # ツール呼び出しがある場合は、ツール実行ノードへ遷移
        print("[SPATIAL NODE]ツールを呼ぶ")
        return NODE.SR_TOOL.value
    else:
        # ツール呼び出しがない場合は、後処理ノードへ遷移
        print("[SPATIAL NODE]ツールを呼ばない")
        return NODE.SR_POSTPROCESS.value

def sr_postprocess_node(state):
    last_msg = state.spatialAgent.messages[-1]
    output = last_msg.content

    state.logger.log(
        node="sr_postprocess_node",
        node_type="postprocess",
        input_data=last_msg.content,
        output_data=output,
        output_type="parsed_text"
    )

    state.spatialAgent.final_output = output
    return state
