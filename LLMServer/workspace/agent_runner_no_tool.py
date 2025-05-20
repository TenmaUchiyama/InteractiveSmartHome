from io import BytesIO
from langgraph.graph import Graph, START, END, StateGraph
from IPython.display import Image, display
from sr_app_types.no_tool_agent_types import State
from sr_app_types.node_types import NODE
from agents.device_filter_agent.filter_nodes_no_tool import filter_preprocess


graph = StateGraph(State)


# =======  ADD NODES =============
# Add filter nodes
graph.add_node(NODE.FILTER_PREPROCESS.value, filter_preprocess)
# graph.add_node(NODE.FILTER_AGENT.value,filter_agent_node )
# graph.add_node(NODE.FILTER_TOOL.value, filter_tool_node)
# graph.add_node(NODE.FILTER_POSTPROCESS.value, filter_postprocess_node)


# Add spatial reasoning nodes
# graph.add_node(NODE.SR_PREPROCESS.value, sr_preprocess_node)
# graph.add_node(NODE.SR_AGENT.value, sr_agent_node)
# graph.add_node(NODE.SR_POSTPROCESS.value, sr_postprocess_node)

# # Add operator nodes
# graph.add_node(NODE.OPERATOR_PREPROCESS.value, operator_preprocess_node)
# graph.add_node(NODE.OPERATOR_AGENT.value, operator_agent_node)
# graph.add_node(NODE.OPERATOR_TOOL.value, operator_tool_node)
# graph.add_node(NODE.OPERATOR_POSTPROCESS.value, operator_postprocess_node)

# =======  ADD EDGES =============
graph.add_edge(START, NODE.FILTER_PREPROCESS.value)
graph.add_edge(NODE.FILTER_PREPROCESS.value , END)

# graph.add_edge(NODE.FILTER_PREPROCESS.value, NODE.FILTER_AGENT.value)
# graph.add_conditional_edges(NODE.FILTER_AGENT.value, filter_router)
# graph.add_edge(NODE.FILTER_TOOL.value, NODE.FILTER_AGENT.value)


# # graph.add_edge(NODE.FILTER_POSTPROCESS.value, END)

# # graph.add_conditional_edges(NODE.FILTER_POSTPROCESS.value, filter_final_router)
# graph.add_edge(NODE.FILTER_POSTPROCESS.value,NODE.SR_PREPROCESS.value)
# graph.add_edge(NODE.SR_PREPROCESS.value,NODE.SR_AGENT.value)
# graph.add_edge(NODE.SR_AGENT.value, NODE.SR_POSTPROCESS.value)


# graph.add_edge(NODE.SR_POSTPROCESS.value, NODE.OPERATOR_PREPROCESS.value)



# graph.add_edge(NODE.OPERATOR_PREPROCESS.value, NODE.OPERATOR_AGENT.value)
# graph.add_conditional_edges(NODE.OPERATOR_AGENT.value, operator_router)
# graph.add_edge(NODE.OPERATOR_TOOL.value, NODE.OPERATOR_AGENT.value)
# graph.add_edge(NODE.OPERATOR_POSTPROCESS.value, END)




runner  = graph.compile()




import PIL.Image

if __name__ == "__main__":
    img_data = runner.get_graph().draw_mermaid_png()

    img_io = BytesIO(img_data)

    img = PIL.Image.open(img_io)
    img.show()