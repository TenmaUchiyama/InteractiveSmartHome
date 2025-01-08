from langchain.callbacks.base import BaseCallbackHandler

class CustomCallbackHandler(BaseCallbackHandler):
    def on_llm_start(self, serialized, prompts, **kwargs):
        print("\n--- 送信するテキスト ---")
        for i, prompt in enumerate(prompts):
            print(f"Prompt {i + 1}: {prompt}")
        print("\n")

    def on_llm_end(self, response, **kwargs):
        print("\n--- LLMからのレスポンス ---")
        print(response)
        print("\n")