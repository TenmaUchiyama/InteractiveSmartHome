from dataclasses import dataclass, field
from typing import Annotated, List, Optional, Dict
from langchain_core.messages import BaseMessage, HumanMessage
from enum import Enum

from EXPERIMENT.task_manager import ExperimentTaskResultManager





@dataclass
class FilterAgentType:
    input_prompt: BaseMessage
    output: List[BaseMessage] = field(default_factory=list)
    tool_output: Optional[Dict] = Optional 
    devices: List[str] = field(default_factory=list)  

@dataclass
class State:
    user_prompt: HumanMessage = None
    filterAgent: FilterAgentType = field(default_factory=FilterAgentType)

    logger: ExperimentTaskResultManager = None

