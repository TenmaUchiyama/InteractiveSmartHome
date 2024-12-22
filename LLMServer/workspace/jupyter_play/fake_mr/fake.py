from fastapi import FastAPI, Depends
from pydantic import BaseModel
from dataclasses import dataclass, asdict
from uuid import UUID, uuid4
from typing import Optional, Tuple , List
import json
import uvicorn
from dataclasses import dataclass
from typing import List

# Pydanticのモデルを作成
class RequestData(BaseModel):
    function_name: str
    args: list[str]

app = FastAPI()



class FunctionRequest(BaseModel):
    function: str
    order: str
    range: Optional[float] = None  # float型のオプション
    



@dataclass
class DeviceData:
    id: UUID
    name: str
    distance_from_user: float
    position: Tuple[float, float, float]
    angle : float

@dataclass
class ReturnDeviceList:
    deviceList: List[DeviceData]

    def __init__(self, device_list: List[DeviceData]):
        self.deviceList = device_list


@app.post("/")
def get_device(function_request : FunctionRequest):
    device_list = []
    if(function_request.function == "direction"):
        device_list = [
            DeviceData(
                id=uuid4(),
                name= "device1",
                distance_from_user = 1.3,
                position=(1.0, 2.0, 3.0),
                angle = 90
            ),
            DeviceData(
                id=uuid4(),
                name= "device2",
                distance_from_user = 2.1,
                position=(4.0, 5.0, 6.0),
                angle = 180
            ),
            DeviceData(
                id=uuid4(),
                name= "device3",
                distance_from_user = 3.9,
                position=(7.0, 8.0, 9.0),
                angle = 270
            )
        ]

        
    

    if(function_request.function == "sight"):
        device_list = [
            DeviceData(
                id=uuid4(),
                name= "device4",
                distance_from_user = 9.12,
                position=(13, 3.3, 3.0),
                angle = 0
            ),
            DeviceData(
                id=uuid4(),
                name= "device5",
                distance_from_user = 3.2,
                position=(3.1, 1.3, 2.0),
                angle = 90
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
    id: List[str]  # デバイスIDのリストを受け取る

@app.post("/operate")
async def operate(input_data: RequestData):
    print("received operation data: ", input_data.dict())
    return {"status": "success", "received_device_ids": input_data.device_id}


if __name__ == "__main__":
    uvicorn.run("fake:app", host="localhost", port=7070, reload=True)
