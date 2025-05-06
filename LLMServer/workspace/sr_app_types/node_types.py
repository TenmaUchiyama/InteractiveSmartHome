from enum import Enum


class NODE(Enum):
    FILTER_PREPROCESS = "filter_preprocess_node"
    FILTER_AGENT = "filter_agent_node"
    FILTER_TOOL= "filter_tool_node"
    FILTER_POSTPROCESS = "filter_postprocess_node"

    SR_PREPROCESS = "sr_preprocess_node"
    SR_AGENT = "sr_agent_node"
    SR_POSTPROCESS = "sr_postprocess_node"
    SR_TOOL="sr_tool_node"

    OPERATOR_PREPROCESS = "operator_preprocess_node"
    OPERATOR_AGENT = "operator_agent_node"
    OPERATOR_TOOL = "operator_tool_node"
    OPERATOR_POSTPROCESS = "operator_postprocess_node"
    
