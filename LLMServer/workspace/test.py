from dotenv import load_dotenv # type: ignore
print(load_dotenv("../.env"))
import os
from no_tool_agent_runner import getFilterDeviceRunner, getSpatialRunner, getSystemRunner
from sr_app_types.no_tool_agent_types import SpatialAgentType, State
from langchain_core.messages import BaseMessage, HumanMessage
from sr_app_types.no_tool_agent_types import FilterAgentType
import json
from agents.device_operator_agent.operator_tool import operateDevice
from utils.communication.DeviceOperator import DeviceOperator


def testFilterDeviceRunner():
    propmt = "日本の信号の色の配色を目の前の電気を使って教えて"
    state: State = State(
        user_prompt=HumanMessage(propmt)
        
    )


    runner = getFilterDeviceRunner()
    output = runner.invoke(state)

    print(output["filterAgent"].devices)




def testSpatialRunner():
    propmt = "日本の信号の色の配色を目の前の電気を使って教えて"

    devices =  [{'id': 'light 3', 'name': 'Ceiling Light 1,0', 'position': [0.0, 2.5, -0.75], 'distance_from_user': 2.6101}, {'id': 'light 4', 'name': 'Ceiling Light 1,1', 'position': [0.0, 2.5, 0.75], 'distance_from_user': 2.6101}, {'id': 'light 1', 'name': 'Ceiling Light 0,0', 'position': [-1.5, 2.5, -0.75], 'distance_from_user': 3.0104}, {'id': 'light 2', 'name': 'Ceiling Light 0,1', 'position': [-1.5, 2.5, 0.75], 'distance_from_user': 3.0104}, {'id': 'light 5', 'name': 'Ceiling Light 2,0', 'position': [1.5, 2.5, -0.75], 'distance_from_user': 3.0104}, {'id': 'light 6', 'name': 'Ceiling Light 2,1', 'position': [1.5, 2.5, 0.75], 'distance_from_user': 3.0104}]

    state: State = State(
        user_prompt=HumanMessage(propmt),
        filterAgent=FilterAgentType(devices=devices, selected_tool={ "filter_type": "fov", "params": {'isInFov': True, 'order': 'proximity', 'range': 0.0}})

    )
   
    print("=========[SR PREPROCESS NODE]=========")

    runner = getSpatialRunner()
    output = runner.invoke(state)
    print(output["spatialAgent"].output_data)



def testSystemRunner():
    propmt = "日本の信号の色の配色を目の前の電気を使って教えて"

    state: State = State(
        user_prompt=HumanMessage(propmt)
    )

    runner = getSystemRunner()
    output = runner.invoke(state)   

    print(output["spatialAgent"].output_data)








deviceOperator = DeviceOperator()

deviceOperator.send_operator([{"id": "light 1", "name": "Ceiling Light 0,0", "position": [0.0, 2.5, -0.75], "distance_from_user": 3.0104}])