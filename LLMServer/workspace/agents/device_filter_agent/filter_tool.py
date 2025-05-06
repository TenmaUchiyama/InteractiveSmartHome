import httpx
from langchain.tools import tool
from langchain_openai import ChatOpenAI
from typing import Annotated, Dict
import os 

base_url = os.getenv("XR_SERVER_API")






@tool
def getDeviceInFov(
    isInFov: Annotated[bool, """- isInFov (bool): If True, returns devices that are within the user's line of sight. Default: True"""],
    order: Annotated[str, """- order (str): Sorting method for devices. Possible values: "proximity", "right", "high". Default: proximity"""],
    range: Annotated[float, """- range (float): Distance range (meters). Input 0.0 if specification is not required."""] 
) -> Dict:
    """
    This function retrieves devices that are within the user's line of sight.
    return boolean value indicates whether the devices are successfully received in back side.
    """
    
    try:
        # print("\n[TOOL FOV] getDevicesInFov")
        
        request_body = {
            "isInFov": isInFov,
            "order": order
        }

        if range != 0.0:
            request_body["range"] = range

        # print(f"Sending POST request to {base_url}/fov with body: {request_body}")
        response = httpx.post(f"{base_url}/fov", json=request_body)
        print("Received Devices")

        if response.status_code == 200:
            response_data = response.json()

            # `param` が存在しない場合は作成する
            response_data.setdefault("param", {})

            # `param` に値を追加
            response_data["param"]["filter_type"] = "fov"
            response_data["param"]["isInFov"] = isInFov
            response_data["param"]["order"] = order
            if range != 0.0:
                response_data["param"]["range"] = range

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
    direction: Annotated[str, """Direction relative to the user's body. Must be one of: 'Front', 'Back', 'Left', 'Right' ,'Up' ,'Down'."""],
    order: Annotated[str, """Sorting order of devices. Options: 'proximity' (closest first), 'right' (from left to right), 'high' (from lowest to highest). Default: 'proximity'"""],
    range: Annotated[float, """Optional distance range in meters. Use 0.0 if not specifying a limit."""] 
) -> Dict:

    """
    This function retrieves devices based on the user's direction and order.

    return boolean value indicates whether the devices are successfully received in backside.
    """


    request_body = {
        "direction": direction,
        "order": order,
        "range" : range
    }
    
    try:
        print(f"Sending POST request to {base_url}/device/direction with body: {request_body}")
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
        order: Annotated[str, """- order (str): Sorting method for devices. Possible values: "proximity", "right", "high". Default: proximity"""],
        range: Annotated[float, """- range (float): Distance range (meters). Input None if specification is not required."""] 
) -> Dict:
    """
    This function retrieves devices based on the user's retrieved direction.
    return boolean value indicates whether the devices are successfully received.
    """
    try:
        # print()
        # print("=====================[TOOL] getDevices================")
        request_body = {
        "order": order ,
        }



        
        if range != 0: 
            request_body["range"] = range
        
        # print(f"Sending POST request to {base_url}/all with body: {request_body}")
        response = httpx.post(f"{base_url}/all", json=request_body)
        
        if response.status_code == 200:
            response_data = response.json()
            response_data.setdefault("param", {})
            if response_data.get("status") == "success":
                print(response_data)
                response_data["param"]["filter_type"] = "all"
                response_data["param"]["order"] = order
                if range != 0:
                    response_data["param"]["range"] = range
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
    furniture_type: Annotated[str, """Type of furniture serving as the reference point. Must be 'TV',  'TABLE' or 'SHELF'."""],
    range: Annotated[float, """Optional distance range around the furniture in meters. Default is 4 meters if not specified."""]
) -> Dict:
    """
    Retrieves devices around specified furniture items.
    Use when the user's instruction explicitly refers to furniture (e.g., "near the TV", "on the table", "around the shelf").
    """
    try:
        request_body = {
            "furniture_type": furniture_type,
             "range" : 5 if range is None else range,
        }
        print(f"Sending POST request to {base_url}/around_furniture with body: {request_body}")
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






