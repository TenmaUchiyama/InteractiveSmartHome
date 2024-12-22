from fastapi import FastAPI, Depends
from pydantic import BaseModel
from dataclasses import dataclass, asdict
from uuid import UUID, uuid4
from typing import Tuple , List
import json
import uvicorn
from dataclasses import dataclass
from typing import List

# Pydanticのモデルを作成
class RequestData(BaseModel):
    function_name: str
    args: list[str]

app = FastAPI()



@dataclass
class FunctionRequest:
    function_name: str
    args: List[str]

    def __init__(self, function_name: str, args: List[str]):
        self.function_name = function_name
        self.args = args



@dataclass
class DeviceData:
    device_id: UUID
    distance_from_user: float
    device_position: Tuple[float, float, float]

@dataclass
class ReturnDeviceList:
    deviceList: List[DeviceData]

    def __init__(self, device_list: List[DeviceData]):
        self.deviceList = device_list


@app.post("/")
def get_device(function_request: FunctionRequest):

    if(function_request.function_name == "direction"):
        print("DIREDCTION: ", function_request.args)
        device_list = [
            DeviceData(
                device_id=uuid4(),
                distance_from_user = 1.3,
                device_position=(1.0, 2.0, 3.0)
            ),
            DeviceData(
                device_id=uuid4(),
                distance_from_user = 2.1,
                device_position=(4.0, 5.0, 6.0)
            ),
            DeviceData(
                device_id=uuid4(),
                distance_from_user = 3.9,
                device_position=(7.0, 8.0, 9.0)
            )
        ]

        
    

    if(function_request.function_name == "sight"):
        device_list = [
            DeviceData(
                device_id=uuid4(),
                distance_from_user = 9.12,
                device_position=(13, 3.3, 3.0)
            ),
            DeviceData(
                device_id=uuid4(),
                distance_from_user = 3.2,
                device_position=(3.1, 1.3, 2.0)
            )
        ]



    device_list.sort(key=lambda x: x.distance_from_user)

    return_data = {
        "status" : "success",
        "body" : device_list
    }

    print("RETURNING DEVICE LIST: ", return_data)
    return return_data

class RequestData(BaseModel):
    device_id: List[str]  # デバイスIDのリストを受け取る

@app.post("/operate")
async def operate(input_data: RequestData):
    print("received operation data: ", input_data.dict())
    return {"status": "success", "received_device_ids": input_data.device_id}


if __name__ == "__main__":
    uvicorn.run(app, host="localhost", port=7070)
