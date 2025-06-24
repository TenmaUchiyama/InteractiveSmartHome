# time_tracker.py
import time

class TimeTracker:
    def __init__(self):
        self.llm_start_time = None
        self.system_start_time = None
        self.system_end_time = None

    def start_llm(self):
        self.llm_start_time = time.time()

    def end_llm(self):
        if self.llm_start_time is None:
            return 0.0
        return time.time() - self.llm_start_time

    def start_system(self):
        self.system_start_time = time.time()

    def end_system(self):
        self.system_end_time = time.time()
        if self.system_start_time is None:
            return 0.0
        return self.system_end_time - self.system_start_time
