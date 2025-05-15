
from typing import Annotated, Dict, List, Optional, Union
from pydantic import BaseModel, Field
from langchain.tools import tool


from utils.communication.DeviceOperator import DeviceOperator


class RGBColor(BaseModel):
    r: int = Field(..., description="Red (0-255)")
    g: int = Field(..., description="Green (0-255)")
    b: int = Field(..., description="Blue (0-255)")

class DeviceControlData(BaseModel):
    id: str = Field(..., description="Device ID to control.")
    state: bool = Field(..., description="Power state. True = ON, False = OFF.")
    intensity: int = Field(..., description="Brightness level from 0 to 100.")
    color: RGBColor = Field(..., description="Color as RGB values.")



deviceOperator = DeviceOperator()



@tool
def operateDevice(   devices: List[DeviceControlData]) -> str:
    """
    This function operates devices based on provided control data.
    **For curtain, 0 = open, 100 = close    
    """


    try:

        print()
        print("=====================[OPERATOR TOOL] operateDevice=====================")
        # デバイスデータを取得
        convert_data =  [device.dict() for device in devices]
        
        response = deviceOperator.send_operator(convert_data)
        print("RESPONSE: ", response)
        return  response
        
    
    except Exception as e:
        print("ERROR OCCURRED DURING OPERATION TOOL: ", e)
        return f"エラーが発生しました: {e}"
















# if __name__ == "__main__":
#     print(format_tool_to_openai_function())