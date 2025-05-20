from agent_runner_no_tool import runner
from workspace.sr_app_types.no_tool_agent_types import State



state: State = State(
        user_prompt="一番右の電気を点けてください",
      
    )


runner.invoke(state)


