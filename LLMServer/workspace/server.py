

from http.client import HTTPException
import json
from fastapi import FastAPI
from starlette.middleware.cors import CORSMiddleware
import uvicorn
from pydantic import BaseModel
import dotenv
import os 
dotenv.load_dotenv()
from llm.apps.llm_app import invoke_llm_agent
from llm.apps.pointing import invoke_pointing_agent
from llm.apps.label import invoke_label_agent
from llm.apps.multiple_selections import invoke_multiple_selection_agent
from log_writer import add_label_log, add_pointing_log, add_spatial_log


app = FastAPI()

# CORS Middleware Configuration
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Adjust as needed
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


class Message(BaseModel):
    llm_message: str
    task_id: str

@app.post("/label_agent")
def pointing_agent(message : Message):
    try:
        print("\n\n")
        print("INPUT: ", message.llm_message)
        print("\n\n")
        res = invoke_label_agent(message.llm_message)
        return_value = {
            "llm_response": res["response"]
        }

        add_label_log(message.task_id, res["messages"])


        return return_value
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Internal server error: {str(e)}")

@app.post("/pointing_agent")
def pointing_agent(message : Message):
    try:
        print("\n\n")
        print("INPUT: ", message.llm_message)
        print("\n\n")
        response = invoke_pointing_agent(message.llm_message)
        return_value = {
            "llm_response": response["response"]
        }

        add_pointing_log(message.task_id, response["messages"])
        return return_value
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Internal server error: {str(e)}")
    

@app.post("/multiple_select_agent")
def pointing_agent(message : Message):
    try:
        print("\n\n")
        print("INPUT: ", message.llm_message)
        print("\n\n")
        response = invoke_multiple_selection_agent(message.llm_message)
        return_value = {
            "llm_response": response
        }
        return return_value
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Internal server error: {str(e)}")

@app.post("/llm_agent")
def llm_agent(message: Message):
    try:
        # LLMエージェントにメッセージを送信し、レスポンスを取得
        print("\n\n")
        print("INPUT: ", message.llm_message)
        print("\n\n")
        
        response = invoke_llm_agent(message.llm_message)

        return_value = {
            "llm_response": response["response"]
        }

        add_spatial_log(message.task_id, response["messages"])
        return return_value

    except Exception as e:
        # エラーハンドリング：何らかの問題が発生した場合
        raise HTTPException(status_code=500, detail=f"Internal server error: {str(e)}")


if __name__ == "__main__":
    uvicorn.run("server:app", host="localhost", port=8800, reload=True)