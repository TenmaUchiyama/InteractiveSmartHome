from io import BytesIO
from langgraph.graph import Graph, START, END, StateGraph
from IPython.display import Image, display
from sr_app_types.agent_types import State
from sr_app_types.node_types import NODE
from agents.spatial_reasoning_agent.spatial_nodes import spatial_router, spatial_tool_node, sr_postprocess_node, sr_preprocess_node, sr_agent_node
from agents.device_filter_agent.filter_nodes import filter_final_router, filter_preprocess_node, filter_agent_node, filter_router, filter_tool_node, filter_postprocess_node
from agents.device_operator_agent.operator_node import operator_agent_node, operator_postprocess_node, operator_preprocess_node, operator_router, operator_tool_node

# グラフ定義
graph = StateGraph(State)

# =======  ADD NODES =============
# フィルタリング系ノード
graph.add_node(NODE.FILTER_PREPROCESS.value, filter_preprocess_node)
graph.add_node(NODE.FILTER_AGENT.value, filter_agent_node)
graph.add_node(NODE.FILTER_TOOL.value, filter_tool_node)
graph.add_node(NODE.FILTER_POSTPROCESS.value, filter_postprocess_node)

# 空間推論系ノード
graph.add_node(NODE.SR_PREPROCESS.value, sr_preprocess_node)
graph.add_node(NODE.SR_AGENT.value, sr_agent_node)
graph.add_node(NODE.SR_TOOL.value, spatial_tool_node)
graph.add_node(NODE.SR_POSTPROCESS.value, sr_postprocess_node)

# =======  ADD EDGES =============
graph.add_edge(START, NODE.FILTER_PREPROCESS.value)
graph.add_edge(NODE.FILTER_PREPROCESS.value, NODE.FILTER_AGENT.value)
graph.add_conditional_edges(NODE.FILTER_AGENT.value, filter_router)
graph.add_edge(NODE.FILTER_TOOL.value, NODE.FILTER_AGENT.value)

graph.add_edge(NODE.FILTER_POSTPROCESS.value,NODE.SR_PREPROCESS.value)
graph.add_edge(NODE.SR_PREPROCESS.value, NODE.SR_AGENT.value)
graph.add_conditional_edges(NODE.SR_AGENT.value, spatial_router)
graph.add_edge(NODE.SR_TOOL.value, NODE.SR_AGENT.value)

# 終了
graph.add_edge(NODE.SR_POSTPROCESS.value, END)


runner = graph.compile()