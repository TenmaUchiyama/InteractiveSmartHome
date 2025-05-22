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
        # ... 既存の初期化処理 ...
        self.last_tokens = None
        self.last_cost = None
        self.last_time = None
        self.start_time = None

    def on_llm_start(self, serialized, prompts, **kwargs):
        self.start_time = time.time()
        # ... 既存コード ...

    def on_llm_end(self, response: LLMResult, **kwargs):
        elapsed_time = time.time() - self.start_time if self.start_time else 0.0
        self.last_time = elapsed_time

        try:
            usage = response.llm_output.get("token_usage", {})
            prompt_tokens = usage.get("prompt_tokens", 0)
            completion_tokens = usage.get("completion_tokens", 0)
            self.last_tokens = {
                "prompt_tokens": prompt_tokens,
                "completion_tokens": completion_tokens,
                "total_tokens": prompt_tokens + completion_tokens
            }

            cost_prompt = 0.01  # GPT-4 Turbo
            cost_completion = 0.03
            cost = (prompt_tokens / 1000) * cost_prompt + (completion_tokens / 1000) * cost_completion
            self.last_cost = round(cost, 6)

        except Exception as e:
            self.last_tokens = None
            self.last_cost = None
