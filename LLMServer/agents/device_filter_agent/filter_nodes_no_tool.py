def filter_tool_node(state : State):
    print("=========[FILTER TOOL NODE]=========")
    print(state.filterAgent.selected_tool)
    
    if state.filterAgent.selected_tool is not None:
        tool = filter_tool_map[state.filterAgent.selected_tool["filter_type"]]
        params = state.filterAgent.selected_tool["params"]
        # paramsを辞書として渡す
        result = tool.invoke(params)
        return result
    else:
        return None 