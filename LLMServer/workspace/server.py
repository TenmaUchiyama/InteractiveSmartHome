from typing import Any, Dict, List
import dotenv
dotenv.load_dotenv("../.env")
from utils.communication.SwitchBotOperator import SwitchBotOperator

import os
from sr_app_types.no_tool_agent_types import LabelState, PointingState, State
from no_tool_agent_runner import getLabelRunner, getPointingRunner, getPointingSpatialRunner, getSystemRunner
from fastapi import FastAPI # type: ignore
import httpx # type: ignore
from starlette.middleware.cors import CORSMiddleware # t    ype: ignore
import uvicorn# type: ignore
from pydantic import BaseModel# type: ignore
import os 
import json
from dataclasses import asdict
from langchain_core.messages import BaseMessage

from utils.time_tracker import TimeTracker


OUTPUT_FILE_NAME="TEST"
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

def run_agent_and_log(state, runner, task_id, attempt_id, save_file_path, pop_keys=None, additional_data: dict = None, isTutorial = False):
    try:

        print(save_file_path)
        response = runner.invoke(state)
        if hasattr(state, "time_tracker") and state.time_tracker is not None:
            print("SYSTEM に　時間トラッキングがある")
            
            system_duration = state.time_tracker.end_system()
            print("SYSTEM DURATION:", system_duration)
        else:
            print("SYSTEM に　時間トラッキングがない")
            system_duration = None
            
        print(response.get("agent_output"))
        agent_output = response.get("agent_output")
        if not agent_output:
            return {"error": "No agent output produced."}
        serialized = remove_keys_flat(response,pop_keys)
        print("SERIALIZED:" , serialized)
        serialized = serialize_value(serialized)
        if system_duration is not None:
            serialized["system_total_time_sec"] = system_duration



        serialized["task_id"] = task_id
        serialized["attempt_id"] = attempt_id
        if additional_data:
            serialized.update(additional_data)

        #　もしチュートリアルであれば保存しない
        if isTutorial:
            return {"output": agent_output.get("response"), "reasoning": agent_output.get("reasoning")}

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
    timer_tracker = TimeTracker()
    timer_tracker.start_system()
    state = PointingState(
        user_prompt=message.llm_message,
        pointed_devices=message.pointed_devices,
        time_tracker=timer_tracker
    )
    runner = getPointingRunner()
    save_path = make_save_path("3")
    return run_agent_and_log(
        state, runner, message.task_id, message.attempt_id, save_path,
        pop_keys=["input_prompt", "all_devices", "time_tracker", "callback"],
        additional_data={"pointed_devices": message.pointed_devices}
    )

@app.post("/label")
def llm_label(message: InputMessage):
    print("LABEL")
    timer_tracker = TimeTracker()
    timer_tracker.start_system()
    state = LabelState(user_prompt=message.llm_message,time_tracker=timer_tracker)
    runner = getLabelRunner()
    save_path = make_save_path("4")
    return run_agent_and_log(
        state, runner, message.task_id, message.attempt_id, save_path,
        pop_keys=["input_prompt", "all_devices","time_tracker"]
    )
@app.post("/llm_agent")
def llm_agent_no_tool(message: InputMessage):
    print("SpatialReference")

    time_tracker = TimeTracker()
    time_tracker.start_system()

    state = State(user_prompt=message.llm_message, time_tracker=time_tracker)
    runner = getSystemRunner()
    save_path = make_save_path("1")

    return run_agent_and_log(
        state, runner, message.task_id, message.attempt_id, save_path,
        pop_keys=["filterAgent.input_prompt", "spatialAgent.input_prompt", "time_tracker"]
    )

@app.post("/pointing_spatial_tutorial")
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
            additional_data={"pointed_devices": message.pointed_devices},
            isTutorial=True
        )
    else:
        state = State(user_prompt=message.llm_message)
        runner = getSystemRunner()
        save_path = make_save_path("2")
        return run_agent_and_log(
            state, runner, message.task_id, message.attempt_id, save_path,
            pop_keys=["filterAgent.input_prompt", "spatialAgent.input_prompt"],
            isTutorial=True
        )



@app.post("/pointing_spatial")
def llm_pointing_spatial(message: InputMessageWithPointing):
    print("SR POINTING")
    is_pointing = message.pointed_devices and len(message.pointed_devices) > 0


    time_tracker = TimeTracker()
    time_tracker.start_system()
    if is_pointing:
        state = PointingState(
            user_prompt=message.llm_message,
            pointed_devices=message.pointed_devices,
            time_tracker=time_tracker
        )
        print("POINTING SPATIAL STATE:" )
        runner = getPointingSpatialRunner()
        save_path = make_save_path("2")



        
        return run_agent_and_log(
            state, runner, message.task_id, message.attempt_id, save_path,
            pop_keys=["input_prompt", "all_devices","callback", "time_tracker"],
            additional_data={"pointed_devices": message.pointed_devices}
        )
    else:
        state = State(user_prompt=message.llm_message, time_tracker=time_tracker)
        runner = getSystemRunner()
        save_path = make_save_path("2")
        return run_agent_and_log(
            state, runner, message.task_id, message.attempt_id, save_path,
            pop_keys=["filterAgent.input_prompt", "spatialAgent.input_prompt", "time_tracker"]
        )




if __name__ == "__main__":
    uvicorn.run("server:app", host="localhost", port=8800, reload=True)
