


from typing import Dict, Optional
from fastapi import FastAPI
from pydantic import BaseModel
from starlette.middleware.cors import CORSMiddleware
import uvicorn
import dotenv
import os 
dotenv.load_dotenv()

app = FastAPI()

# CORS Middleware Configuration
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Adjust as needed
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


test_devices = [
    {"id": "light 1", "name": "Ceiling Light 0,0", "position": (-1.5, 2.5, -0.75), "distance_from_user": 3.0104},
    {"id": "light 2", "name": "Ceiling Light 0,1", "position": (-1.5, 2.5, 0.75), "distance_from_user": 3.0104},
    {"id": "light 3", "name": "Ceiling Light 1,0", "position": (0.0, 2.5, -0.75), "distance_from_user": 2.6101},
    {"id": "light 4", "name": "Ceiling Light 1,1", "position": (0.0, 2.5, 0.75), "distance_from_user": 2.6101},
    {"id": "light 5", "name": "Ceiling Light 2,0", "position": (1.5, 2.5, -0.75), "distance_from_user": 3.0104},
    {"id": "light 6", "name": "Ceiling Light 2,1", "position": (1.5, 2.5, 0.75), "distance_from_user": 3.0104},
]

@app.get("/test")
def sight():
    return {"status" : "success", "devices" : test_devices}


class FOVRequest(BaseModel):
    isInFov: bool
    order: str
    range: Optional[float] = None  # range を nullable に

class DirectionRequest(BaseModel):
    direction: str
    order: str
    range: Optional[float] = None  # range を nullable に


class AllRequest(BaseModel):
    order: str
    range: Optional[float] = None 


class DeviceControlData(BaseModel):
    id: str
    state: bool
    intensity: int
    color: Optional[Dict[str, int]] 


class AroundFurnitureRequest(BaseModel):
    furniture_type: str
    range: Optional[float] = None



@app.post("/device/all")
def all(body : AllRequest):
    print(body)
    return {"status" : "success", "devices" : sort_devices(body.order, test_devices)}

@app.post("/device/fov")
def sight(body : FOVRequest):
    print(body)
    return {"status" : "success", "devices" : sort_devices(body.order, test_devices)}



@app.post("/device/direction")
def direction(body : DirectionRequest):
    print(body)
    return {"status" : "success", "devices" : sort_devices(body.order, test_devices)}


@app.post("/device/operate")
def operate(body : list[DeviceControlData]):
    print("OPERATION")
    print("COUNT: ", len(body))
    print(body)
    return {"status" : "success", "message" : "All Devices Are Operated Successfully."}


@app.post("/device/around_furniture")
def around_furniture(body : AroundFurnitureRequest):
    print(body)
    return {"status" : "success", "devices" : sort_devices(body.order, test_devices)}





def sort_devices(order: str, devices: list):
    if order == "proximity":
        return sorted(devices, key=lambda d: d["distance_from_user"])
    elif order == "right":
        return sorted(devices, key=lambda d: d["position"][0], reverse=True)  # x の値が大きい順
    elif order == "height":
        return sorted(devices, key=lambda d: d["position"][2], reverse=True)  # z の値が大きい順
    else:
        return devices 
    



if __name__ == "__main__":
    uvicorn.run("test_server:app", host="localhost", port=7070, reload=True)