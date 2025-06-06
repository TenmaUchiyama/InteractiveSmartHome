
import os, sys
test_data = [
  {
    "input": "Turn on the device behind me",
    "output": {
      "filter_type": "direction",
      "params": {
        "direction": "Back",
        "order": "proximity",
        "range": 0.0
      },
      "reasoning": "The phrase 'behind me' indicates a body-relative direction, so 'direction' with 'Back' was selected."
    }
  },
  {
    "input": "Switch off all the visible lights",
    "output": {
      "filter_type": "fov",
      "params": {
        "isInFov": True,
        "order": "proximity",
        "range": 0.0
      },
      "reasoning": "The term 'visible' suggests a viewpoint-based spatial layout, so 'fov' was selected."
    }
  },
  {
    "input": "Turn on the light to my left",
    "output": {
      "filter_type": "direction",
      "params": {
        "direction": "Left",
        "order": "proximity",
        "range": 0.0
      },
      "reasoning": "The instruction specifies 'to my left', which is a body-relative term."
    }
  },
  {
    "input": "Activate the topmost light",
    "output": {
      "filter_type": "fov",
      "params": {
        "isInFov": True,
        "order": "high",
        "range": 0.0
      },
      "reasoning": "The word 'topmost' refers to vertical spatial order in the field of view."
    }
  },
  {
    "input": "Turn off the farthest fan",
    "output": {
      "filter_type": "fov",
      "params": {
        "isInFov": True,
        "order": "proximity",
        "range": 0.0
      },
      "reasoning": "The phrase 'farthest' implies spatial ordering in the visible layout."
    }
  },
  {
    "input": "Turn on the lights above me",
    "output": {
      "filter_type": "direction",
      "params": {
        "direction": "Up",
        "order": "proximity",
        "range": 0.0
      },
      "reasoning": "The instruction includes the body-relative phrase 'above me'."
    }
  },
  {
    "input": "Enable everything nearby",
    "output": {
      "filter_type": "all",
      "params": {
        "order": "proximity",
        "range": 2.0
      },
      "reasoning": "The phrase 'nearby' implies a proximity-based filter, so 'all' was selected with a non-zero range."
    }
  },
  {
    "input": "Turn on all lights",
    "output": {
      "filter_type": "all",
      "params": {
        "order": "proximity",
        "range": 0.0
      },
      "reasoning": "The instruction targets all devices of a type, so the 'all' filter with default range is used."
    }
  },
  {
    "input": "What is the temperature right now?",
    "output": {
      "filter_type": None,
      "params": None,
      "reasoning": "The instruction is a status query and does not require device filtering."
    }
  },
  {
    "input": "Dim the rightmost light",
    "output": {
      "filter_type": "fov",
      "params": {
        "isInFov": True,
        "order": "right",
        "range": 0.0
      },
      "reasoning": "The instruction uses 'rightmost', indicating visible layout ordering."
    }
  },
  {
    "input": "Turn on the closest device I can see",
    "output": {
      "filter_type": "fov",
      "params": {
        "isInFov": True,
        "order": "proximity",
        "range": 0.0
      },
      "reasoning": "The reference to visibility and closeness suggests spatial filtering in the field of view."
    }
  },
  {
    "input": "Illuminate the lights below me",
    "output": {
      "filter_type": "direction",
      "params": {
        "direction": "Down",
        "order": "proximity",
        "range": 0.0
      },
      "reasoning": "The phrase 'below me' is a body-relative spatial direction."
    }
  },
  {
    "input": "Turn on the fan in front of me",
    "output": {
      "filter_type": "direction",
      "params": {
        "direction": "Front",
        "order": "proximity",
        "range": 0.0
      },
      "reasoning": "The phrase 'in front of me' is body-relative, so the direction filter was selected."
    }
  },
  {
    "input": "Shut off the farthest visible air conditioner",
    "output": {
      "filter_type": "fov",
      "params": {
        "isInFov": True,
        "order": "proximity",
        "range": 0.0
      },
      "reasoning": "The instruction involves spatial layout in the visible area with 'farthest visible'."
    }
  },
  {
    "input": "Deactivate everything within two meters",
    "output": {
      "filter_type": "all",
      "params": {
        "order": "proximity",
        "range": 2.0
      },
      "reasoning": "The range constraint 'within two meters' indicates a proximity filter over all devices."
    }
  },
  {
    "input": "What devices are active?",
    "output": {
      "filter_type": None,
      "params": None,
      "reasoning": "The instruction asks for status and does not involve device selection."
    }
  },
  {
    "input": "Turn on every device I can see",
    "output": {
      "filter_type": "fov",
      "params": {
        "isInFov": True,
        "order": "proximity",
        "range": 0.0
      },
      "reasoning": "The phrase 'I can see' implies visible field-based device filtering."
    }
  },
  {
    "input": "Switch off the top-right light",
    "output": {
      "filter_type": "fov",
      "params": {
        "isInFov": True,
        "order": "right",
        "range": 0.0
      },
      "reasoning": "The spatial reference 'top-right' relates to visual layout, suggesting a 'fov' filter."
    }
  },
  {
    "input": "Disable the leftmost fan",
    "output": {
      "filter_type": "fov",
      "params": {
        "isInFov": True,
        "order": "right",
        "range": 0.0
      },
      "reasoning": "Although 'leftmost' is used, the layout reference is from the visible field, so 'fov' is used with 'right' ordering for selection."
    }
  },
  {
    "input": "Say good morning",
    "output": {
      "filter_type": None   ,
      "params": None,
      "reasoning": "The instruction is a verbal-only action and does not involve any device filtering."
    }
  }
]

sys.path.append(os.path.dirname(os.path.abspath(__file__)) + "/..")
from dotenv import load_dotenv
load_dotenv("../.env")
from no_tool_agent_runner import getFilterRunner
from sr_app_types.no_tool_agent_types import State


runner = getFilterRunner()

state = State(
    user_prompt = "目の前の手前の電気を点けて",
)



res = runner.invoke(state) 



print(res)






