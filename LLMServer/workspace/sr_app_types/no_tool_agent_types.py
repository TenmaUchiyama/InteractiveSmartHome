from dataclasses import dataclass, field
from typing import Annotated, List, Optional, Dict
from langchain_core.messages import BaseMessage, HumanMessage
from enum import Enum
from pydantic import BaseModel
from typing import List, Dict, Any






@dataclass 
class LabelState: 
    user_prompt: str
    all_devices: List[Dict[str, Any]] = field(default_factory=list)
    agent_output: Optional[Dict[str, Any]] = None
    reasoning: str 
    selected_devices = field(default_factory=list)  # 選択されたデバイスのリスト

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
    metrics: Optional[Dict] = None  # e.g., accuracy, latency

@dataclass
class SpatialAgentType:
    input_prompt: List[BaseMessage] = field(default_factory=list)
    output_data: Optional[SpatialOutput] = None
    metrics: Optional[Dict] = None  # e.g., is_correct, error_type

@dataclass
class State:
    user_prompt: str
    filterAgent: FilterAgentType = field(default_factory=FilterAgentType)
    spatialAgent: SpatialAgentType = field(default_factory=SpatialAgentType)
    selected_devices: List[str] = field(default_factory=list) 
    system_metrics: Optional[Dict] = None






