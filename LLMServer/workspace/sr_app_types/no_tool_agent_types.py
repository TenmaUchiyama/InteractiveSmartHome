from dataclasses import dataclass, field
from typing import Annotated, List, Optional, Dict
from langchain_core.messages import BaseMessage, HumanMessage
from enum import Enum
from pydantic import BaseModel
from typing import List, Dict, Any

from EXPERIMENT.task_manager import ExperimentTaskResultManager

class Position(BaseModel):
    x: float
    y: float
    z: float

class Device(BaseModel):
    id: str
    position: Position

class Color(BaseModel):
    r: int
    g: int
    b: int

class DeviceState(BaseModel):
    id: str
    state: bool
    intensity: int
    color: Color

class SpatialInput(BaseModel):
    filter_selected_tool: Optional[Dict] = None
    user_prompt: str
    devices: List[Device]

class SpatialOutput(BaseModel):
    devices: List[DeviceState]
    response: str

@dataclass
class FilterAgentType:
    input_prompt: List[BaseMessage] = field(default_factory=list)
    selected_tool: Optional[Dict] = None 
    devices: List[str] = field(default_factory=list)  
    metrics: Optional[Dict] = None

@dataclass
class SpatialAgentType:
    output: List[BaseMessage] = field(default_factory=list)
    input_prompt: List[BaseMessage] = field(default_factory=list)
    output_data: Optional[SpatialOutput] = None
    metrics: Optional[Dict] = None

@dataclass
class State:
    user_prompt: HumanMessage = None
    filterAgent: FilterAgentType = field(default_factory=FilterAgentType)
    spatialAgent: SpatialAgentType = field(default_factory=SpatialAgentType)
    logger: ExperimentTaskResultManager = None

