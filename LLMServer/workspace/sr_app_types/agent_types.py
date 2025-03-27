from dataclasses import dataclass, field
from typing import List, Optional, Dict
from langchain_core.messages import BaseMessage, HumanMessage
from enum import Enum


@dataclass
class FilterAgentType:
    messages: List[BaseMessage] = field(default_factory=list)
    final_output: Optional[Dict] = Optional  
    tool_parameter : Optional[Dict] = Optional
    devices: List[str] = field(default_factory=list)  

@dataclass
class AgentHistory: 
    messages: List[BaseMessage] = field(default_factory=list)
    final_output: Optional[Dict] = None

@dataclass
class State:
    user_prompt: HumanMessage = None
    filterAgent : FilterAgentType = field(default_factory = FilterAgentType)
    spatialAgent: AgentHistory = field(default_factory = AgentHistory)
    operatorAgent: AgentHistory = field(default_factory = AgentHistory)

class AGENT(Enum): 
    DEVICE_FILTER = "filterAgent"
    SPATIAL_REASONING= "spatialReasoningAgent"
    DEVICE_OPERATION="deviceOperationAgent"