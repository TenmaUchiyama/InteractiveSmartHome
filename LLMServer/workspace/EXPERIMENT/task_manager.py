# log_manager.py
import datetime
import os
import json
import threading

class ExperimentTaskResultManager:
    _instance = None
    _lock = threading.Lock()

    def __init__(self):
        self.current_task_id = None
        self.logs = []

    @classmethod
    def instance(cls):
        with cls._lock:
            if cls._instance is None:
                cls._instance = cls()
            return cls._instance

    def start(self, task_id: str):
        self.current_task_id = task_id
        self.logs = []

    def log(self, node: str, node_type: str, input_data, output_data, output_type=None):
        self.logs.append({
            "task_id": self.current_task_id,
            "node": node,
            "type": node_type,
            "agent_output_type": output_type,
            "input": input_data,
            "output": output_data,
            "timestamp": datetime.datetime.now().isoformat()
        })

    def save(self, dir_path="logs/task_json"):
        os.makedirs(dir_path, exist_ok=True)
        path = os.path.join(dir_path, f"{self.current_task_id}.json")
        with open(path, "w", encoding="utf-8") as f:
            json.dump(self.logs, f, ensure_ascii=False, indent=2, default=str)
