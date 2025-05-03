import dotenv
<<<<<<< HEAD
from agent_runner import runner
from sr_app_types.agent_types import State
dotenv.load_dotenv("../.env")
=======
dotenv.load_dotenv("../.env")
import os
from agent_runner import runner
from sr_app_types.agent_types import State
>>>>>>> stack
from fastapi import FastAPI
import httpx
from starlette.middleware.cors import CORSMiddleware
import uvicorn
from pydantic import BaseModel
<<<<<<< HEAD
import os 
=======
>>>>>>> stack

XR_SERVER_API = os.getenv("XR_SERVER_API")


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


<<<<<<< HEAD


@app.get("/")
def test():
=======
@app.get("/")
def test():
    print("Hello")
    return "HELO"

@app.get("/simple")
def simple():
>>>>>>> stack

    user_prompt = "Turn on all the lights i can see"
    state : State = State(
        user_prompt = user_prompt,
    )

    response = runner.invoke(state)

    return {"output" : response['spatialAgent'].final_output}



@app.post("/llm_agent")
<<<<<<< HEAD
def llm_agent(message : InputMessage):
    user_prompt = message.llm_message
    task_id = message.task_id
    state : State = State(
        user_prompt = user_prompt,
    )
    response = runner.invoke(state)
    output = response['operatorAgent'].final_output
    print("********************OUTPUT*********************")
    print(output)

    return {"output" : output}
=======
def llm_agent(message: InputMessage):
    user_prompt = message.llm_message
    task_id = message.task_id
    state: State = State(
        user_prompt=user_prompt,
    )

    try:
        response = runner.invoke(state)
        output = response['operatorAgent'].final_output
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
>>>>>>> stack
    

if __name__ == "__main__":

    print("APIKEY: ",os.getenv("OPENAI_API_KEY"))
    uvicorn.run("server:app", host="127.0.0.1", port=8800, reload=True)