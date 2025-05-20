from dotenv import load_dotenv # type: ignore
load_dotenv("../.env")
from agent_runner_no_tool import runner
from sr_app_types.no_tool_agent_types import State
from langchain_core.messages import BaseMessage, HumanMessage
from sr_app_types.no_tool_agent_types import FilterAgentType


state: State = State(
    user_prompt=HumanMessage("一番右の電気を点けてください"),
    filterAgent=FilterAgentType(input_prompt=HumanMessage("一番右の電気を点けてください"))
)


runner.invoke(state)


