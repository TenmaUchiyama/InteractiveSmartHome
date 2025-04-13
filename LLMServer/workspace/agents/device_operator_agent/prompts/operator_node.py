from enum import Enum
from langchain_core.messages import ToolMessage
from langchain_openai import ChatOpenAI
from pydantic import BaseModel, Field
from langchain_core.messages import HumanMessage, SystemMessage
from agents.device_operator_agent.prompts.operator_tool import operateDevice
import os
from sr_app_types.node_types import NODE
from utils.callbacks import CustomCallbackHandler


script_dir = os.path.dirname(os.path.abspath(__file__))  # スクリプトのあるディレクトリ
file_path = os.path.join(script_dir, "prompts", "operator_msg.txt")
with open(file_path, "r", encoding="utf-8") as f:
    operator_message_txt = SystemMessage(f.read())

class OPERATOR_TOOL(Enum): 
    OPERATE = "operateDevice" 

callback = CustomCallbackHandler("logs/operator_logs.md")
llm_operator_model = ChatOpenAI(model="gpt-4o", verbose=True, callbacks=[callback]) 
llm_operator = llm_operator_model.bind_tools([operateDevice], strict=True)
tool_map  = {
    OPERATOR_TOOL.OPERATE.value : operateDevice
}




def operator_preprocess_node(state): 
    # print("=======[Operator] PREPROCESS ================")

   
    operator_input_msg = HumanMessage(f"""
    User Input: {state.user_prompt}
    InputFromSpatialReasoningAgent: {state.spatialAgent.final_output}
    """)
    # print(operator_input_msg)

    operaotr_msgs = [
        operator_message_txt,
        operator_input_msg
    ]
    state.operatorAgent.messages = operaotr_msgs
    # print(operator_input_msg)
    return state

def operator_agent_node(state):
    print("=======[Operator] AGENT ================")
    last_msg = state.operatorAgent.messages[-1]
    # if isinstance(last_msg, ToolMessage):
        # print("Last message is a ToolMessage")
        # ToolMessage に対する処理を書く
        # print(last_msg.content)

    agent_output = llm_operator.invoke(state.operatorAgent.messages) 
   
    print(agent_output)
    state.operatorAgent.messages.append(agent_output)
    return state

def operator_tool_node(state): 
    last_msg = state.operatorAgent.messages[-1]
    
    for this_tool in last_msg.tool_calls:
        tool_call_id = this_tool['id']
        tool_function = tool_map.get(this_tool['name'])
        
        if tool_function is None: 
            raise ValueError(f"Tool function not found for {this_tool['name']}")
        print("000000000000 args 000000000000")
        print(this_tool['args'])
        print("000000000000000000000000000000")
        tool_output = tool_function.invoke(this_tool['args'])
            
        # 修正: tool_call_id を適切な引数名で渡す
        tool_message = ToolMessage(
            tool_call_id=tool_call_id,  # 修正
            content=tool_output
        )
        
        state.operatorAgent.messages.append(tool_message)

    
    return state
    

def operator_router(state): 
    last_msg = state.operatorAgent.messages[-1]
    if last_msg.tool_calls:

        return NODE.OPERATOR_TOOL.value
    else:
        # print("ツールは呼ばないよ")
        return NODE.OPERATOR_POSTPROCESS.value

def operator_postprocess_node(state):
    # print("=======[Operator] POSTPROCESS ================")
    last_msg = state.operatorAgent.messages[-1]
    output = last_msg.content
    # print("Operator Output: ", output)
    state.operatorAgent.final_output = output
    return state




    
