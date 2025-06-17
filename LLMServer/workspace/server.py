from typing import Any, Dict, List
import dotenv
from utils.communication.SwitchBotOperator import SwitchBotOperator
dotenv.load_dotenv("../.env")
import os
from sr_app_types.no_tool_agent_types import LabelState, PointingState, State
from no_tool_agent_runner import getLabelRunner, getPointingRunner, getPointingSpatialRunner, getSystemRunner
from fastapi import FastAPI # type: ignore
import httpx # type: ignore
from starlette.middleware.cors import CORSMiddleware # type: ignore
import uvicorn# type: ignore
from pydantic import BaseModel# type: ignore
import os 
import json
from dataclasses import asdict
from langchain_core.messages import BaseMessage


OUTPUT_FILE_NAME="P4"
app = FastAPI()


llm_runner = getSystemRunner()
label_runner = getLabelRunner()
pointing_runner = getPointingRunner()



# CORS Middleware Configuration
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Adjust as needed
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)




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



def remove_keys_flat(obj: dict, keys_to_remove: list):
    return {k: v for k, v in obj.items() if k not in keys_to_remove}





def serialize_value(value):
    if isinstance(value, list):
        return [serialize_value(v) for v in value]
    elif isinstance(value, dict):
        return {k: serialize_value(v) for k, v in value.items()}
    elif isinstance(value, BaseMessage):
        return {
            "type": value.type,
            "content": value.content
        }
    elif hasattr(value, "__dict__"):
        return serialize_value(vars(value))
    elif isinstance(value, (str, int, float, bool, type(None))):
        return value
    else:
        return str(value)  

def run_agent_and_log(state, runner, task_id, attempt_id, save_file_path, pop_keys=None, additional_data: dict = None):
    try:
        response = runner.invoke(state)
      
        agent_output = response.get("agent_output")
        if not agent_output:
            return {"error": "No agent output produced."}
        serialized = remove_keys_flat(response,pop_keys)
        print("SERIALIZED:" , serialized)
        serialized = serialize_value(serialized)
 

        serialized["task_id"] = task_id
        serialized["attempt_id"] = attempt_id
        if additional_data:
            serialized.update(additional_data)

        os.makedirs(os.path.dirname(save_file_path), exist_ok=True)
        if os.path.exists(save_file_path) and os.path.getsize(save_file_path) > 0:
            with open(save_file_path, "r", encoding="utf-8") as f:
                existing_logs = json.load(f)
        else:
            existing_logs = []

        existing_logs.append(serialized)
        with open(save_file_path, "w", encoding="utf-8") as f:
            json.dump(existing_logs, f, indent=2, ensure_ascii=False)

        return {
            "output": agent_output.get("response"),
            "reasoning": agent_output.get("reasoning"),
            "matched_devices": agent_output.get("devices")
        }

    except Exception as e:
        import traceback
        traceback.print_exc()
        return {"error": "Agent processing failed.", "detail": str(e)}



def make_save_path(name: str) -> str:
    return f"../ExperimentData/RESULTS/{OUTPUT_FILE_NAME}/{OUTPUT_FILE_NAME}_{name}.json"


# ----------- 入力データモデル -------------
class InputMessage(BaseModel):
    llm_message: str
    task_id: str
    attempt_id: str

class InputMessageWithPointing(InputMessage):
    pointed_devices: List[Dict[str, Any]]

# ----------- エンドポイント定義 -------------
@app.post("/pointing")
def llm_pointing(message: InputMessageWithPointing):
    print("POINTING")
    state = PointingState(
        user_prompt=message.llm_message,
        pointed_devices=message.pointed_devices
    )
    runner = getPointingRunner()
    save_path = make_save_path("3")
    return run_agent_and_log(
        state, runner, message.task_id, message.attempt_id, save_path,
        pop_keys=["input_prompt", "all_devices"],
        additional_data={"pointed_devices": message.pointed_devices}
    )


@app.post("/pointing_only")
def llm_pointing(message: InputMessageWithPointing):
    print("POINTING")
    state = PointingState(
        user_prompt=message.llm_message,
        pointed_devices=message.pointed_devices
    )
    runner = getPointingRunner()
    save_path = make_save_path("3")
    return run_agent_and_log(
        state, runner, message.task_id, message.attempt_id, save_path,
        pop_keys=["input_prompt", "all_devices"],
        additional_data={"pointed_devices": message.pointed_devices}
    )

@app.post("/label")
def llm_label(message: InputMessage):
    print("LABEL")
    state = LabelState(user_prompt=message.llm_message)
    runner = getLabelRunner()
    save_path = make_save_path("4")
    return run_agent_and_log(
        state, runner, message.task_id, message.attempt_id, save_path,
        pop_keys=["input_prompt", "all_devices"]
    )

@app.post("/llm_agent")
def llm_agent_no_tool(message: InputMessage):
    print("SpatialReference")
    state = State(user_prompt=message.llm_message)
    runner = getSystemRunner()  # Spatial agent runner 呼び出し
    save_path = make_save_path("1")

    try:
        response = runner.invoke(state)
        output = response["spatialAgent"].output_data

        serialized = serialize_value(response)
        for agent in ["filterAgent", "spatialAgent"]:
            if agent in serialized:
                serialized[agent].pop("input_prompt", None)

        serialized["task_id"] = message.task_id
        serialized["attempt_id"] = message.attempt_id

        os.makedirs(os.path.dirname(save_path), exist_ok=True)
        if os.path.exists(save_path) and os.path.getsize(save_path) > 0:
            with open(save_path, "r", encoding="utf-8") as f:
                existing_logs = json.load(f)
        else:
            existing_logs = []

        existing_logs.append(serialized)
        with open(save_path, "w", encoding="utf-8") as f:
            json.dump(existing_logs, f, indent=2, ensure_ascii=False)

        return {
            "output": output.get("response") if output else None
        }

    except Exception as e:
        import traceback
        traceback.print_exc()
        return {
            "error": "LLM processing failed.",
            "detail": str(e)
        }



@app.post("/pointing_spatial")
def llm_pointing_spatial(message: InputMessageWithPointing):
    print("SR POINTING")
    is_pointing = message.pointed_devices and len(message.pointed_devices) > 0

    if is_pointing:
        state = PointingState(
            user_prompt=message.llm_message,
            pointed_devices=message.pointed_devices
        )
        print("POINTING SPATIAL STATE:" )
        runner = getPointingSpatialRunner()
        save_path = make_save_path("2")



        
        return run_agent_and_log(
            state, runner, message.task_id, message.attempt_id, save_path,
            pop_keys=["input_prompt", "all_devices","callback"],
            additional_data={"pointed_devices": message.pointed_devices}
        )
    else:
        state = State(user_prompt=message.llm_message)
        runner = getSystemRunner()
        save_path = make_save_path("2")
        return run_agent_and_log(
            state, runner, message.task_id, message.attempt_id, save_path,
            pop_keys=["filterAgent.input_prompt", "spatialAgent.input_prompt"]
        )




if __name__ == "__main__":
    uvicorn.run("server:app", host="localhost", port=8800, reload=True)
