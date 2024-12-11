
from llm.agent.llm_agent import runner, State
from langchain_core.messages import SystemMessage, HumanMessage




def invoke_llm_agent(user_input: str) -> str:
    state: State = {
        "messages": [
            SystemMessage(content="""
            You are an AI assistant for controlling smart home devices. Based on voice commands from the user, you must identify and control devices considering the user's position, direction, and line of sight. You have access to functions that can retrieve devices based on their spatial relationship to the user.

            Depending on the context of the command, you may need to select a single device or multiple devices to perform the desired action. Your task is to process the user's command and use the available functions to find the appropriate devices, whether it's one or many, and perform the required actions.

            When using any function that returns a list of devices, you **must respect the order of the returned list** as it is pre-sorted based on the user's preferences. 
    
                          

            Finally, when you find the devices or not, find the best function to operate them.
            """)
        ]
    }

    state["messages"].append(HumanMessage(content=user_input))

    
    res = runner.invoke(state)

    return res["messages"][-1].content




if __name__ == "__main__":
    
    user_command = "Turn on the lights in front of me"
    response = invoke_llm_agent(user_command)
    
    print("\n--- 最終的なレスポンス ---")
    print(response)
    print("\n")
