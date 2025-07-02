from io import BytesIO
from agents.label_agent.label_node import label_agent_node, label_tool_node
from langgraph.graph import Graph, START, END, StateGraph
from IPython.display import Image, display
from sr_app_types.no_tool_agent_types import LabelState, State, PointingState
from sr_app_types.node_types import NODE
from agents.device_filter_agent.no_tool_filter_nodes import filter_preprocess, filter_agent_node, filter_tool_node
from agents.spatial_reasoning_agent.no_tool_spatial_node import pointing_spatial_node, sr_agent_node, sr_preprocess_node, sr_tool_node, system_post_process_node
from agents.pointing_agent.point_node import pointing_agent_node, pointing_tool_node





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



def getFilterRunner():
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


def getSpatialRunnerForEvaluation():
    graph = StateGraph(State)

    graph.add_node(NODE.SR_PREPROCESS.value, sr_preprocess_node)
    graph.add_node(NODE.SR_AGENT.value, sr_agent_node)
    
    graph.add_edge(START, NODE.SR_PREPROCESS.value)
    graph.add_edge(NODE.SR_PREPROCESS.value, NODE.SR_AGENT.value)
    graph.add_edge(NODE.SR_AGENT.value, END)
   
    runner  = graph.compile()   
    return runner



def getLabelRunner():
    graph = StateGraph(LabelState)

    # ノード定義
    graph.add_node("label_agent_node", label_agent_node)
    graph.add_node("label_tool_node", label_tool_node)

    # エッジ定義
    graph.add_edge(START, "label_agent_node")
    graph.add_edge("label_agent_node", "label_tool_node")
    graph.add_edge("label_tool_node", END)

    runner = graph.compile()
    return runner



def getPointingRunner():
    graph = StateGraph(PointingState)


    graph.add_node("pointing_agent_node", pointing_agent_node)
    graph.add_node("pointing_tool_node", pointing_tool_node)

    graph.add_edge(START, "pointing_agent_node")
    graph.add_edge("pointing_agent_node", "pointing_tool_node")
    graph.add_edge("pointing_tool_node", END)

    runner = graph.compile()
    return runner



def getPointingSpatialRunner():
    graph = StateGraph(PointingState)

    graph.add_node("pointing_spatial_node", pointing_spatial_node)
    graph.add_node("pointing_spatial_tool_node", pointing_tool_node)

    graph.add_edge(START, "pointing_spatial_node")
    graph.add_edge("pointing_spatial_node", "pointing_spatial_tool_node")
    graph.add_edge("pointing_spatial_tool_node", END)

    runner = graph.compile()
    return runner