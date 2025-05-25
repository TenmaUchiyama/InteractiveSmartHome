import os
import time
from langchain_core.callbacks.base import BaseCallbackHandler
from langchain_core.outputs.llm_result import LLMResult

class LogFileWriter:
    def __init__(self, file_path: str):
        self.file_path = file_path
        base_dir = os.path.dirname(self.file_path)
        os.makedirs(base_dir, exist_ok=True)
    
    def write_log(self, text: str):
        try:
            with open(self.file_path, 'a', encoding='utf-8') as f:
                f.write(text)
                f.flush()
        except Exception as e:
            print(f"[ログファイルへの書き込みに失敗]: {e}")



class CustomCallbackHandler(BaseCallbackHandler):
    def __init__(self, relative_path: str):
        self.relative_path = relative_path
        self.last_tokens = None
        self.last_cost = None
        self.last_time = None
        self.start_time = None

        # モデル価格（USD / 1M tokens）
        self.model_prices_per_million = {
            "gpt-4.1": {"input": 2.00, "output": 8.00},
            "gpt-4.1-mini": {"input": 0.40, "output": 1.60},
            "gpt-4.1-nano": {"input": 0.10, "output": 0.40},
            "gpt-4.5-preview": {"input": 75.00, "output": 150.00},
            "gpt-4o": {"input": 2.50, "output": 10.00},
        }

    def on_llm_start(self, serialized, prompts, **kwargs):
        self.start_time = time.time()
    
    def system_start(self):
        self.system_start_time = time.time()
    
    def system_end(self):
        self.system_end_time = time.time()
        return self.system_end_time - self.system_start_time


    def on_llm_end(self, response: LLMResult, **kwargs):
        elapsed_time = time.time() - self.start_time if self.start_time else 0.0


        self.last_time = elapsed_time
        

        try:
            usage = response.llm_output.get("token_usage", {})
            prompt_tokens = usage.get("prompt_tokens", 0)
            completion_tokens = usage.get("completion_tokens", 0)
            total_tokens = prompt_tokens + completion_tokens
            self.model_name = response.llm_output.get("model_name", "gpt-4o")
            self.last_tokens = {
            
                "prompt_tokens": prompt_tokens,
                "completion_tokens": completion_tokens,
                "total_tokens": total_tokens,
            }

            # モデル取得と単価取得
            model_name = os.getenv("GPT_MODEL") or "gpt-4o"
            prices = self.model_prices_per_million.get(model_name, {"input": 0.0, "output": 0.0})

            input_cost = (prices["input"] / 1_000_000) * prompt_tokens
            output_cost = (prices["output"] / 1_000_000) * completion_tokens

            self.last_cost = round(input_cost + output_cost, 6)

        except Exception as e:
            self.last_tokens = None
            self.last_cost = None
