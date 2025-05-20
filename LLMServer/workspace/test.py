from dotenv import load_dotenv # type: ignore
load_dotenv("../.env")
from agent_runner_no_tool import runner
from sr_app_types.no_tool_agent_types import State
from langchain_core.messages import BaseMessage, HumanMessage
from sr_app_types.no_tool_agent_types import FilterAgentType
from agents.device_filter_agent.filter_nodes_no_tool import filter_tool_map


propmt = "俺の後ろの電気の右の電気を点けて"
state: State = State(
    user_prompt=HumanMessage(propmt),
    filterAgent=FilterAgentType(input_prompt=HumanMessage(propmt))
)


output = runner.invoke(state)
tool = output["filterAgent"].output["filter_type"]

param = output["filterAgent"].output["params"]




print(filter_tool_map[tool].invoke(param))



