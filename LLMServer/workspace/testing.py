
from dotenv import load_dotenv
load_dotenv()

from llm.apps.label import invoke_label_agent
from llm.tools.tools import operateDevice

if __name__ == "__main__":
    inp = "Turn on light1 with blue"


    test_device = [
    {
        "id": "3ecb69f3-8f64-4ba2-9991-6b603880663c",
        "state": True,
        "intensity": 100,
        "color": {"r": 255, "g": 255, "b": 255}  # Optional
    }
]

#     res = operateDevice(test_device)

#     print(res)
    res = invoke_label_agent(f""" 
                            user_command: "{inp}", 
                             device: {test_device}
                             """)


