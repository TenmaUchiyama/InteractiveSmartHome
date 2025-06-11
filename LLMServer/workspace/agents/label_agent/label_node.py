from langchain_openai import ChatOpenAI # type: ignore


from langchain_core.messages import HumanMessage# type: ignore

from langchain_core.output_parsers import JsonOutputParser# type: ignore
from typing import Dict, Literal
from langchain_core.messages import SystemMessage# type: ignore

from agents.device_filter_agent.filter_tool import getDeviceInFov, getDeviceInDirection, getDevices
import os
import json
from utils.callbacks import CustomCallbackHandler
from sr_app_types.no_tool_agent_types import LabelState
from workspace.agents.device_operator_agent.operator_tool import operateDevice



script_dir = os.path.dirname(os.path.abspath(__file__))  # スクリプトのあるディレクトリ
file_path = os.path.join(script_dir, "prompts", "label_msg.txt")
all_device_file_path =  os.path.join(script_dir, "device_list.json")


with open(all_device_file_path, "r", encoding="utf-8") as f:
    all_device = json.load(f)

with open(file_path, "r", encoding="utf-8") as f:
    label_message = SystemMessage(f.read())



callback = CustomCallbackHandler("logs/filter_logs.md")
model = os.getenv("GPT_MODEL")
model = model.strip() if model and model.strip() else "gpt-4o"
label_agent = ChatOpenAI(model= model, temperature= 0.0, callbacks=[callback])

parser = JsonOutputParser(pydantic_object=Dict)



def label_agent_node(label_state: LabelState):

    """
    ラベルエージェントの実行関数
    """
    # ユーザープロンプトとタスクIDを取得
    user_prompt = label_state.user_prompt


    
    input_user_prompt = f"""
    ALL DEVICES: {json.dumps(all_device, ensure_ascii=False)}
    USER PROMPT: {user_prompt}
    
    """
    input_prompt = [
        label_message,
        HumanMessage(content=input_user_prompt)
    ]
    
    print(input_prompt)
    response = label_agent.invoke(
        input_prompt
    ) 

    print("========OUTPUT========")
    print(response.content)

    parsed_output = parser.parse(response.content)




    label_state.agent_output = parsed_output

    return label_state




def label_tool_node(state: LabelState):
    # print("=========[SR TOOL NODE]=========")
  
    devices = state.agent_output.get("devices", [])
    result = operateDevice.invoke(devices)
    return state