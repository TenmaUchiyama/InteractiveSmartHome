
from http.client import HTTPException
from fastapi import FastAPI
from starlette.middleware.cors import CORSMiddleware
from llm_app import invoke_llm_agent
import uvicorn
from pydantic import BaseModel


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



@app.post("/llm_agent")
def llm_agent(message: Message):
    try:
        # LLMエージェントにメッセージを送信し、レスポンスを取得
        response = invoke_llm_agent(message.llm_message)

        # レスポンスをJSON形式で返す
        return_value = {
            "llm_response": response
        }
        return return_value

    except Exception as e:
        # エラーハンドリング：何らかの問題が発生した場合
        raise HTTPException(status_code=500, detail=f"Internal server error: {str(e)}")


if __name__ == "__main__":
    uvicorn.run("server:app", host="localhost", port=8800, reload=True)