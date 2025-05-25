# === 共通ログクラス ===
class ExecutionLogger:
    def __init__(self):
        self.records = []

    def log(self, model_name, prompt_tokens, completion_tokens, elapsed_seconds):
        self.records.append({
            "model_name": model_name,
            "prompt_tokens": prompt_tokens,
            "completion_tokens": completion_tokens,
            "total_tokens": prompt_tokens + completion_tokens,
            "elapsed_seconds": elapsed_seconds,
        })

    def summary(self):
        total_tokens = sum(r["total_tokens"] for r in self.records)
        total_time = sum(r["elapsed_seconds"] for r in self.records)
        cost = 0.0
        for r in self.records:
            prices = {
                "gpt-4.1-nano": {"input": 0.10, "output": 0.40},
                "gpt-4o": {"input": 2.50, "output": 10.00},
            }
            p = prices.get(r["model_name"].split("-")[0], {"input": 0, "output": 0})
            cost += (p["input"] * r["prompt_tokens"] + p["output"] * r["completion_tokens"]) / 1_000_000
        return {
            "tokens": total_tokens,
            "cost_usd": cost,
            "elapsed_seconds": total_time,
        }
