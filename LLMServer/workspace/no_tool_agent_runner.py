from io import BytesIO
from langgraph.graph import Graph, START, END, StateGraph
from IPython.display import Image, display
from sr_app_types.no_tool_agent_types import State
from sr_app_types.node_types import NODE
from agents.device_filter_agent.no_tool_filter_nodes import filter_preprocess, filter_agent_node, filter_tool_node
from agents.spatial_reasoning_agent.no_tool_spatial_node import sr_agent_node, sr_preprocess_node, sr_tool_node





def getSystemRunner():
    graph = StateGraph(State)
    graph.add_node(NODE.FILTER_PREPROCESS.value, filter_preprocess)
    graph.add_node(NODE.FILTER_AGENT.value,filter_agent_node )
    graph.add_node(NODE.FILTER_TOOL.value, filter_tool_node)

    graph.add_node(NODE.SR_PREPROCESS.value, sr_preprocess_node)
    graph.add_node(NODE.SR_AGENT.value, sr_agent_node)
    graph.add_node(NODE.SR_TOOL.value, sr_tool_node)

    graph.add_edge(START, NODE.FILTER_PREPROCESS.value)
    graph.add_edge(NODE.FILTER_PREPROCESS.value, NODE.FILTER_AGENT.value)
    graph.add_edge(NODE.FILTER_AGENT.value, NODE.FILTER_TOOL.value)
    graph.add_edge(NODE.FILTER_TOOL.value, NODE.SR_PREPROCESS.value)


    graph.add_edge(NODE.SR_PREPROCESS.value, NODE.SR_AGENT.value)
    graph.add_edge(NODE.SR_AGENT.value, NODE.SR_TOOL.value)
    graph.add_edge(NODE.SR_TOOL.value, END) 


    runner  = graph.compile()   
    return runner



def getFilterDeviceRunner():
    graph = StateGraph(State)

    graph.add_node(NODE.FILTER_PREPROCESS.value, filter_preprocess)
    graph.add_node(NODE.FILTER_AGENT.value, filter_agent_node)
    graph.add_node(NODE.FILTER_TOOL.value, filter_tool_node)

    graph.add_edge(START, NODE.FILTER_PREPROCESS.value)
    graph.add_edge(NODE.FILTER_PREPROCESS.value, NODE.FILTER_AGENT.value)
    graph.add_edge(NODE.FILTER_AGENT.value, NODE.FILTER_TOOL.value)
    graph.add_edge(NODE.FILTER_TOOL.value, END)

    runner  = graph.compile()   
    return runner


def getSpatialRunner():
    graph = StateGraph(State)

    graph.add_node(NODE.SR_PREPROCESS.value, sr_preprocess_node)
    graph.add_node(NODE.SR_AGENT.value, sr_agent_node)
    graph.add_node(NODE.SR_TOOL.value, sr_tool_node)

    graph.add_edge(START, NODE.SR_PREPROCESS.value)
    graph.add_edge(NODE.SR_PREPROCESS.value, NODE.SR_AGENT.value)
    graph.add_edge(NODE.SR_AGENT.value, NODE.SR_TOOL.value)
    graph.add_edge(NODE.SR_TOOL.value, END)

    # graph.add_edge(START, NODE.SR_PREPROCESS.value)
    # graph.add_edge(NODE.SR_PREPROCESS.value, NODE.SR_AGENT.value)
    # graph.add_edge(NODE.SR_AGENT.value, NODE.SR_POSTPROCESS.value)
    # graph.add_edge(NODE.SR_POSTPROCESS.value, END)

    runner  = graph.compile()   
    return runner




