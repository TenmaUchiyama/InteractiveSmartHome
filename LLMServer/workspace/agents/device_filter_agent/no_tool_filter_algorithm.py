import httpx
from langchain.tools import tool
from langchain_openai import ChatOpenAI
from typing import Annotated, Dict, Union
import os 

base_url = os.getenv("XR_SERVER_API")






@tool
def getDeviceInFov(
    params: Dict
) -> Dict:
    """
    This function retrieves devices that are within the user's line of sight.
    return boolean value indicates whether the devices are successfully received in back side.
    """
    
    try:
        # print("\n[TOOL FOV] getDevicesInFov")
 
        request_body = {
            "isInFov": params["isInFov"],
            "order": params["order"]
        }

        if params["range"] != 0.0:
            request_body["range"] = params["range"]

        # print(f"Sending POST request to {base_url}/fov with body: {request_body}")
        response = httpx.post(f"{base_url}/device/fov", json=request_body)

        if response.status_code == 200:
            response_data = response.json()

            # `param` が存在しない場合は作成する
            response_data.setdefault("param", {})
            # print(response_data)
            # `param` に値を追加
            response_data["param"]["filter_type"] = "fov"
            response_data["param"]["isInFov"] = params["isInFov"]
            response_data["param"]["order"] = params["order"]
            if params["range"] != 0.0:
                response_data["param"]["range"] = params["range"]

            if response_data.get("status") == "success":
                # print(f"[TOOL FOV] {response_data}")
                return response_data
            else:
                return {"status": "error", "message": "Server responded with an error", "details": response_data}
        else:
            return {"status": "error", "message": f"HTTP Error {response.status_code}", "details": response.text}

    except Exception as e:
        return {"status": "error", "message": str(e)}




@tool
def getDeviceInDirection(
    params: Dict[str, Union[str, float]]
) -> Dict:
    """
    This function retrieves devices based on the user's direction and order.

    return boolean value indicates whether the devices are successfully received in backside.
    """

    direction = params["direction"]
    order = params["order"]
    range = params["range"]

    request_body = {
        "direction": direction,
        "order": order,
        "range": range
    }

    try:
        # print(f"Sending POST request to {base_url}/device/direction with body: {request_body}")
        response = httpx.post(f"{base_url}/device/direction", json=request_body)

        if response.status_code == 200:
            response_data = response.json()
            response_data.setdefault("param", {})
            if response_data.get("status") == "success":
                response_data["param"]["filter_type"] = "direction"
                response_data["param"]["direction"] = direction
                response_data["param"]["order"] = order
                if range != 0:
                    response_data["param"]["range"] = range

                return response_data
            else:
                return {"status": "error", "message": "Server responded with an error", "details": response_data}
        else:
            return {"status": "error", "message": f"HTTP Error {response.status_code}", "details": response.text}
    except Exception as e:
        return {"status": "error", "message": str(e)}
    





@tool
def getDevices(
    params: Dict[str, Union[str, float]]
) -> Dict:
    """
    This function retrieves devices based on the user's retrieved direction.
    return boolean value indicates whether the devices are successfully received.
    """
    try:


    
        # print()
        # print("=====================[TOOL] getDevices================")
        request_body = {
        "order": params["order"] ,
        }



        if params["range"] != 0: 
            request_body["range"] = params["range"]

        # print(f"Sending POST request to {base_url}/all with body: {request_body}")
        response = httpx.post(f"{base_url}/device/all", json=request_body)
        
        if response.status_code == 200:
            response_data = response.json()
            response_data.setdefault("param", {})
            if response_data.get("status") == "success":
                # print(response_data)
                response_data["param"]["filter_type"] = "all"
                response_data["param"]["order"] = params["order"]
                if params["range"] != 0:
                    response_data["param"]["range"] = params["range"]
                # print("================================================")
                # print()
                return response_data
            else:
                return {"status": "error", "message": "Server responded with an error", "details": response_data}
        else:
            return {"status": "error", "message": f"HTTP Error {response.status_code}", "details": response.text}
    except Exception as e:
        return {"status": "error", "message": str(e)}
    

@tool
def getDeviceAroundFurniture(
    params: Dict[str, Union[str, float]],
) -> Dict:
    """
    Retrieves devices around specified furniture items.
    Use when the user's instruction explicitly refers to furniture (e.g., "near the TV", "on the table", "around the shelf").
    """
    try:
     
        request_body = {
            "furniture_type": params["furniture_type"],
            "range": 5 if params["range"] is None else params["range"],
        }
        # print(f"Sending POST request to {base_url}/around_furniture with body: {request_body}")
        response = httpx.post(f"{base_url}/furniture/get", json=request_body)
        if response.status_code == 200:
            response_data = response.json()
            response_data.setdefault("param", {})
            if response_data.get("status") == "success":
                output["param"]["filter_type"] = "around_furniture"
                output["status"] = response_data.get('status')
                output["devices"] = response_data.get("devices")
               
                return output
            else:
                return {"status": "error", "message": "Server responded with an error", "details": response_data}
        else:
            return {"status": "error", "message": f"HTTP Error {response.status_code}", "details": response.text}
    except Exception as e:
        return {"status": "error", "message": str(e)}






