import dotenv
from utils.communication.SwitchBotOperator import SwitchBotOperator
dotenv.load_dotenv("../.env")
import os
from sr_app_types.no_tool_agent_types import State
from no_tool_agent_runner import getSystemRunner
from fastapi import FastAPI # type: ignore
import httpx # type: ignore
from starlette.middleware.cors import CORSMiddleware # type: ignore
import uvicorn# type: ignore
from pydantic import BaseModel# type: ignore
import os 
import json
from dataclasses import asdict
from langchain_core.messages import BaseMessage


OUTPUT_FILE_NAME="P2"
app = FastAPI()



class InputMessage(BaseModel):
    llm_message: str
    task_id: str



# CORS Middleware Configuration
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Adjust as needed
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)




class InputMessage(BaseModel):
    llm_message: str
    task_id: str


@app.get("/")
def test():
    print("Hello")
    return "HELO"





@app.get("/simple") 
def simple():

    user_prompt = "Turn on all the lights i can see"
    state : State = State(
        user_prompt = user_prompt,
    )

    response = runner.invoke(state)

    return {"output" : response['spatialAgent'].final_output}



@app.post("/label")
def llm_label(message: InputMessage):
    """
    受け取ったメッセージをラベル付けするエンドポイント
    """
    user_prompt = message.llm_message
    task_id = message.task_id
    

    return labeled_data

def serialize_value(value):
    if isinstance(value, BaseMessage):
        return {
            "type": value.type,
            "content": value.content,
            # 追加で必要なら他の属性も
        }
    elif isinstance(value, list):
        return [serialize_value(v) for v in value]
    elif isinstance(value, dict):
        return {k: serialize_value(v) for k, v in value.items()}
    elif hasattr(value, "__dataclass_fields__"):
        return serialize_value(asdict(value))
    else:
        return value


@app.post("/llm_agent")
def llm_agent_no_tool(message: InputMessage):

    runner = getSystemRunner()
    user_prompt = message.llm_message
    task_id = message.task_id
    print("PROMPT: ", user_prompt)
    print("ID: ", task_id)

    state = State(user_prompt=user_prompt)

    try:
        response = runner.invoke(state)
        output = response["spatialAgent"].output_data
        serialized = serialize_value(response)

        if "filterAgent" in serialized:
            serialized["filterAgent"].pop("input_prompt", None)
        if "spatialAgent" in serialized:
            serialized["spatialAgent"].pop("input_prompt", None)
        
        serialized["task_id"] = task_id

        save_path = f"../ExperimentData/RESULTS/{OUTPUT_FILE_NAME}.json"
        os.makedirs(os.path.dirname(save_path), exist_ok=True)

        # 既存ログを読み込み（なければ空リスト）
        if os.path.exists(save_path) and os.path.getsize(save_path) > 0:
            with open(save_path, "r", encoding="utf-8") as f:
                existing_logs = json.load(f)
        else:
            existing_logs = []

        # データを追加
        existing_logs.append(serialized)

        # ファイルに書き戻し
        with open(save_path, "w", encoding="utf-8") as f:
            json.dump(existing_logs, f, indent=2, ensure_ascii=False)

        return {
            "output": serialized["spatialAgent"]["output_data"]["response"] if output else None
        }

    except Exception as e:
        import traceback
        print("************ EXCEPTION OCCURRED **************")
        traceback.print_exc()
        return {
            "error": "LLM processing failed.",
            "detail": str(e)
        }




if __name__ == "__main__":
    uvicorn.run("server:app", host="localhost", port=8800, reload=True)
