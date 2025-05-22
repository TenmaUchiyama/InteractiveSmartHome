import json
import os
from typing import List, Dict, Any, Optional, Tuple

import numpy as np
from sklearn.metrics import (
    confusion_matrix,
    classification_report,
)
from no_tool_agent_runner import getFilterDeviceRunner
from sr_app_types.no_tool_agent_types import State


class FilterAgentEvaluator:
    def __init__(
        self,
        data_path: str,
        runner: Optional[Any] = None,
    ):
        """
        data_path: fov.json のパス
        runner:      getFilterDeviceRunner() の戻り値（省略時は内部で生成）
        """
        self.data_path = data_path
        self.runner = runner or getFilterDeviceRunner()
        self._load_data()
        self.outputs: List[Dict[str, Any]] = []

    def _load_data(self):
        """JSONファイルからテストケースを読み込み、self.data に格納"""
        with open(self.data_path, "r", encoding="utf-8") as f:
            self.data: List[Dict[str, Any]] = json.load(f)

    def run_tests(self, start: int = 0, end: Optional[int] = None) -> List[Dict[str, Any]]:
        """
        start から end-1 まで順番にエージェントを呼び出し、self.outputs に結果を蓄積して返す
        """
        end = end or len(self.data)
        for idx in range(start, min(end, len(self.data))):
            item = self.data[idx]
            state = State(user_prompt=item["user_prompt"])
            res = self.runner.invoke(state)
            fa = res["filterAgent"]

            output = {
                "id": item["id"],
                "ground_truth": item["output"],
                "predicted": fa.selected_tool,
                "metrics": fa.metrics,
            }
            self.outputs.append(output)
        return self.outputs

    def save_outputs(self, path: str):
        """run_tests の outputs を JSON 形式で保存"""
        os.makedirs(os.path.dirname(path), exist_ok=True)
        with open(path, "w", encoding="utf-8") as f:
            json.dump(self.outputs, f, ensure_ascii=False, indent=2)

    def _extract_labels(self) -> Tuple[List[str], List[str]]:
        """ground truth / predicted の filter_type をそれぞれリストで返す"""
        y_true = [o["ground_truth"]["filter_type"] for o in self.outputs]
        y_pred = [o["predicted"]["filter_type"] for o in self.outputs]
        return y_true, y_pred

    def compute_confusion_matrix(self, labels: Optional[List[str]] = None) -> np.ndarray:
        """
        混同行列を返す。labels にクラス順リストを渡すと固定順で出力可能
        """
        y_true, y_pred = self._extract_labels()
        return confusion_matrix(y_true, y_pred, labels=labels)

    def classification_report(self, labels: Optional[List[str]] = None) -> str:
        """
        Precision/Recall/F1 を含むレポート文字列を返す
        """
        y_true, y_pred = self._extract_labels()
        return classification_report(y_true, y_pred, labels=labels, zero_division=0)

    def compute_accuracy(self) -> float:
        """完全一致率 (filter_type + params 全一致) を返す"""
        total = len(self.outputs)
        correct = 0
        for o in self.outputs:
            if o["predicted"] == o["ground_truth"]:
                correct += 1
        return correct / total if total else 0.0

    def compute_partial_match(self) -> float:
        """
        filter_type 一致を部分一致とみなす率を返す
        (完全一致 + params 差異あり) / 全体
        """
        total = len(self.outputs)
        type_match = 0
        for o in self.outputs:
            if o["predicted"]["filter_type"] == o["ground_truth"]["filter_type"]:
                type_match += 1
        return type_match / total if total else 0.0

    def cost_summary(self) -> Dict[str, float]:
        """
        metrics.cost_usd の合計・平均・最大を返す
        """
        costs = [o["metrics"].get("cost_usd", 0.0) for o in self.outputs if o["metrics"]]
        if not costs:
            return {"total": 0.0, "average": 0.0, "max": 0.0}
        return {
            "total": sum(costs),
            "average": sum(costs) / len(costs),
            "max": max(costs),
        }

    def evaluate_all(self, labels: Optional[List[str]] = None) -> Dict[str, Any]:
        """
        主な評価指標をまとめて返す
        {
          "accuracy": ...,
          "partial_match": ...,
          "confusion_matrix": np.ndarray,
          "classification_report": str,
          "cost_summary": {...}
        }
        """
        return {
            "accuracy": self.compute_accuracy(),
            "partial_match": self.compute_partial_match(),
            "confusion_matrix": self.compute_confusion_matrix(labels),
            "classification_report": self.classification_report(labels),
            "cost_summary": self.cost_summary(),
        }
