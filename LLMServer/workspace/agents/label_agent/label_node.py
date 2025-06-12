from typing import Dict
from langchain_core.messages import HumanMessage, SystemMessage
from langchain_core.output_parsers import JsonOutputParser
from langchain_openai import ChatOpenAI
from utils.callbacks import CustomCallbackHandler
from sr_app_types.no_tool_agent_types import LabelState
import os, json

from agents.device_operator_agent.operator_tool import operateDevice



# プロンプトと全デバイスの読み込み
script_dir = os.path.dirname(os.path.abspath(__file__))
file_path = os.path.join(script_dir, "prompts", "label_msg.txt")
device_list_path = os.path.join(script_dir, "device_list.json")

with open(file_path, "r", encoding="utf-8") as f:
    label_message = SystemMessage(f.read())

with open(device_list_path, "r", encoding="utf-8") as f:
    all_devices = json.load(f)

# モデルとコールバック設定
callback = CustomCallbackHandler("logs/label_logs.md")
model = os.getenv("GPT_MODEL") or "gpt-4o"
label_agent = ChatOpenAI(model=model.strip(), temperature=0.0, callbacks=[callback])

parser = JsonOutputParser(pydantic_object=Dict)

def label_agent_node(state: LabelState):
    callback.system_start()
    
    state.all_devices = all_devices  # ← 忘れずに状態に反映

    input_content = {
        "user_prompt": state.user_prompt,
        "devices": state.all_devices
    }

    state.input_prompt = [
        label_message,
        HumanMessage(content=json.dumps(input_content, ensure_ascii=False))
    ]

    response = label_agent.invoke(state.input_prompt)

    print("========[LABEL AGENT OUTPUT]========")
    print(response.content)

    parsed = parser.parse(response.content)
    state.agent_output = parsed
    state.metrics = {
        "model_name": callback.model_name,
        "tokens": callback.last_tokens,
        "cost_usd": callback.last_cost,
        "agent_time_elapsed": callback.last_time
    }
    return state




def label_tool_node(state: LabelState):
    devices = state.agent_output.get("devices", [])
    result = operateDevice.invoke({"devices": devices})
    state.metrics["system_time_elapsed"] = callback.system_end()
    return state
