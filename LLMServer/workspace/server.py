import dotenv
from EXPERIMENT.task_manager import ExperimentTaskResultManager
dotenv.load_dotenv("../.env")
import os
from agent_runner_spoperate import runner
from sr_app_types.agent_types import State
from fastapi import FastAPI
import httpx
from starlette.middleware.cors import CORSMiddleware
import uvicorn
from pydantic import BaseModel
import os 



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



@app.post("/llm_agent")
def llm_agent(message: InputMessage):


    
    user_prompt = message.llm_message
    task_id = message.task_id
    print("ID: ", task_id)
    # グローバルLogger初期化（インスタンス共有）
    logger = ExperimentTaskResultManager.instance()
    logger.start(task_id)

    # loggerをStateに注入してrunnerに渡す
    state: State = State(
        user_prompt=user_prompt,
        logger=logger
    )

    try:
        response = runner.invoke(state)
        output = response['spatialAgent'].final_output

        # 成功後ログ書き出し
        logger.save()

        print("********************OUTPUT*********************")
        print(output)
        return {"output": output}

    except Exception as e:
        # 例外の詳細ログ
        import traceback
        print("************ EXCEPTION OCCURRED **************")
        traceback.print_exc()

        return {
            "error": "LLM processing failed.",
            "detail": str(e)
        }
    

if __name__ == "__main__":
    uvicorn.run("server:app", host="localhost", port=8800, reload=True)
