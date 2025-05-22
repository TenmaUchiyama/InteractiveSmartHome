from dotenv import load_dotenv # type: ignore
print(load_dotenv("../.env"))
import json
from no_tool_agent_runner import getFilterDeviceRunner 
from sr_app_types.agent_types import State

evalTestData = {
    "fov" : json.load(open("../Evaluation/TestData/en/short/fov.json")),
    "direction" : json.load(open("../Evaluation/TestData/en/short/direction.json")),
    "around_furniture" : json.load(open("../Evaluation/TestData/en/short/around_furniture.json")),
    "all" : json.load(open("../Evaluation/TestData/en/short/all.json")),
    "null" : json.load(open("../Evaluation/TestData/en/short/null.json")),
}



state = State(
    user_prompt=evalTestData['fov'][0]['user_prompt']
)

runner = getFilterDeviceRunner()
res = runner.invoke(state)  


print("USER_PROMPT: ", evalTestData['fov'][0]['user_prompt'])
print("SYSTEM_PROMPT: ", res['filterAgent'])