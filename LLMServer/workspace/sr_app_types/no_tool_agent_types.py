from dataclasses import dataclass, field
from typing import List, Optional, Dict, Any
from langchain_core.messages import BaseMessage
from utils.time_tracker import TimeTracker


@dataclass
class PointingAgentOutput:
    selected_devices: List[Dict[str, Any]]
    response: str
    reasoning: str


@dataclass 
class PointingState: 
    input_prompt: List[BaseMessage] = field(default_factory=list)
    user_prompt: str = ""
    pointed_devices: List[Dict[str, Any]] = field(default_factory=list)
    agent_output: Optional[PointingAgentOutput] = None
    metrics: Optional[Dict[str, Any]] = None
    time_tracker: TimeTracker = field(default_factory=TimeTracker)


@dataclass 
class LabelAgentOutput:
    selected_devices: List[Dict[str, Any]]
    response: str
    reasoning: str


@dataclass 
class LabelState:
    input_prompt: List[BaseMessage] = field(default_factory=list)
    user_prompt: str = ""
    all_devices: List[Dict[str, Any]] = field(default_factory=list)
    agent_output: Optional[LabelAgentOutput] = None
    metrics: Optional[Dict[str, Any]] = None
    time_tracker: TimeTracker = field(default_factory=TimeTracker)


@dataclass
class FilterAgentOutput:
    filter_type: str
    params: Dict[str, Any]
    reasoning: str


@dataclass
class SpatialOutput:
    devices: List[Dict[str, Any]]
    response: str
    reasoning: str
    response_time_ms: Optional[float] = None


@dataclass
class FilterAgentType:
    input_prompt: List[BaseMessage] = field(default_factory=list)
    output_tool_selection: Optional[FilterAgentOutput] = None
    devices: Optional[List[Dict[str, Any]]] = None
    metrics: Optional[Dict[str, Any]] = None


@dataclass
class SpatialAgentType:
    input_prompt: List[BaseMessage] = field(default_factory=list)
    output_data: Optional[SpatialOutput] = None
    metrics: Optional[Dict[str, Any]] = None


@dataclass
class State:
    user_prompt: str
    filterAgent: FilterAgentType = field(default_factory=FilterAgentType)
    spatialAgent: SpatialAgentType = field(default_factory=SpatialAgentType)
    agent_output: Optional[SpatialOutput] = None
    selected_devices: List[str] = field(default_factory=list)
    system_metrics: Optional[Dict[str, Any]] = None
    time_tracker: TimeTracker = field(default_factory=TimeTracker)  # ← 追加