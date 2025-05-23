import os
from typing import List, Dict, Any, Optional
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
from sklearn.metrics import confusion_matrix, classification_report

from sklearn.metrics import precision_recall_fscore_support
sns.set(style="whitegrid")


class Analyzer:
    def __init__(self, data: List[Dict[str, Any]], ran_data: List[Dict[str, Any]]):
        self.data = data
        self.ran_data = ran_data

    # -----------------------
    # 評価メソッド
    # -----------------------

    def compute_exact_match_accuracy(self) -> float:
        total = len(self.ran_data)
        correct = sum(1 for o in self.ran_data if o["predicted"] == o["ground_truth"])
        return correct / total if total else 0.0
    
    def compute_type_accuracy(self) -> float:
        """filter_type 一致率"""
        total = len(self.ran_data)
        correct = sum(1 for o in self.ran_data
                    if o["predicted"].get("filter_type") == o["ground_truth"].get("filter_type"))
        return correct / total if total else 0.0

    def compute_partial_match(self) -> float:
        total = len(self.ran_data)
        type_match = sum(
            1 for o in self.ran_data
            if o["predicted"]["filter_type"] == o["ground_truth"]["filter_type"]
        )
        return type_match / total if total else 0.0

    def compute_confusion_matrix(self, labels: Optional[List[str]] = None) -> np.ndarray:
        y_true, y_pred = self._extract_labels()
        return confusion_matrix(y_true, y_pred, labels=labels)

    def classification_report(self, labels: Optional[List[str]] = None) -> str:
        y_true, y_pred = self._extract_labels()
        return classification_report(y_true, y_pred, labels=labels, zero_division=0)

    def cost_summary(self) -> Dict[str, float]:
        costs = [o["metrics"].get("cost_usd", 0.0) for o in self.ran_data if o.get("metrics")]
        if not costs:
            return {"total": 0.0, "average": 0.0, "max": 0.0}
        return {
            "total": sum(costs),
            "average": sum(costs) / len(costs),
            "max": max(costs)
        }

    def time_summary(self) -> Dict[str, float]:
        times = [o["metrics"].get("elapsed_seconds", 0.0) for o in self.ran_data if o.get("metrics")]
        if not times:
            return {"total": 0.0, "average": 0.0, "max": 0.0}
        return {
            "total": sum(times),
            "average": sum(times) / len(times),
            "max": max(times)
        }

    def evaluate_all(self, labels: Optional[List[str]] = None) -> Dict[str, Any]:
        result = {
            "exact_match_accuracy": self.compute_exact_match_accuracy(),
            "type_accuracy": self.compute_type_accuracy(),
            "partial_match": self.compute_partial_match(),
            "confusion_matrix": self.compute_confusion_matrix(labels),
            "classification_report": self.classification_report(labels),
            "cost_summary": self.cost_summary(),
            "time_summary": self.time_summary(),
        }

        # FovAnalyzerなどの派生クラスでparameter評価が定義されていれば追加
        if hasattr(self, "compute_param_metrics"):
            result["parameter_metrics"] = self.compute_param_metrics()

        return result

    def _extract_labels(self) -> (List[str], List[str]): # type: ignore
        y_true = [o["ground_truth"]["filter_type"] for o in self.ran_data]
        y_pred = [o["predicted"]["filter_type"] for o in self.ran_data]
        return y_true, y_pred

    # -----------------------
    # 表示・可視化メソッド
    # -----------------------

    def display_overview(self):
        id_to_info = {
            item["id"]: {
                "task_id": item.get("task_id", "(no task_id)"),
                "user_prompt": item.get("user_prompt", "(no prompt)")
            } for item in self.data
        }

        def analyze_param_match(gt: Dict, pred: Dict) -> str:
            matched = []
            unmatched = []

            if pred.get("isInFov") == gt.get("isInFov"):
                matched.append("isInFov")
            else:
                unmatched.append("isInFov")

            if pred.get("order") == gt.get("order"):
                matched.append("order")
            else:
                unmatched.append("order")

            if abs(pred.get("range", 0.0) - gt.get("range", 0.0)) < 1e-4:
                matched.append("range")
            else:
                unmatched.append("range")

            result = ""
            if matched:
                result += f"✅ {matched}"
            if unmatched:
                result += f" ❌ {unmatched}"
            return result

        df = pd.DataFrame([{
            "Task ID": id_to_info.get(o["id"], {}).get("task_id", "(not found)"),
            "Prompt": id_to_info.get(o["id"], {}).get("user_prompt", "(not found)"),
            "Ground Truth": o["ground_truth"]["filter_type"],
            "Predicted": o["predicted"]["filter_type"],
            "✅ Exact Match": "✅" if o["predicted"] == o["ground_truth"] else "❌",
            "✅ filter_type_match": "✅" if o["predicted"]["filter_type"] == o["ground_truth"]["filter_type"] else "❌",
            "param_match": (
                analyze_param_match(
                    o["ground_truth"].get("params", {}),
                    o["predicted"].get("params", {})
                ) if o["predicted"]["filter_type"] == o["ground_truth"]["filter_type"] else "—"
            ),
            "Cost (USD)": o["metrics"].get("cost_usd", None),
            "Response Time (s)": o["metrics"].get("elapsed_seconds", None),
            "Model": o.get("used_model")
        } for o in self.ran_data])

        print(df.to_markdown(index=False))

        return df




    def plot_confusion_matrix(self, labels: Optional[List[str]] = None):
        y_true, y_pred = self._extract_labels()
        labels = labels or sorted(set(y_true) | set(y_pred))
        cm = confusion_matrix(y_true, y_pred, labels=labels)
        cm_df = pd.DataFrame(cm, index=labels, columns=labels)
        print("Confusion Matrix:")
        print(cm_df.to_markdown())

        plt.figure(figsize=(6, 5))
        sns.heatmap(cm_df, annot=True, fmt="d", cmap="Blues")
        plt.title("Confusion Matrix Heatmap")
        plt.xlabel("Predicted")
        plt.ylabel("True")
        plt.tight_layout()
        plt.show()

    def plot_cost_distribution(self):
        costs = [o["metrics"].get("cost_usd", 0.0) for o in self.ran_data]
        plt.figure(figsize=(8, 4))
        sns.histplot(costs, bins=20, kde=True)
        plt.title("Cost Distribution (USD)")
        plt.xlabel("Cost (USD)")
        plt.ylabel("Frequency")
        plt.tight_layout()
        plt.show()

    def plot_scatter_cost_time(self):
        times = [o["metrics"].get("time_s") for o in self.ran_data]
        costs = [o["metrics"].get("cost_usd") for o in self.ran_data]
        plt.figure(figsize=(6, 4))
        sns.scatterplot(x=times, y=costs,
                        hue=[o["predicted"] == o["ground_truth"] for o in self.ran_data])
        plt.title("Response Time vs Cost")
        plt.xlabel("Time (s)")
        plt.ylabel("Cost (USD)")
        plt.tight_layout()
        plt.show()

    def plot_cost_by_match(self):
        df = pd.DataFrame([{
            "Exact Match": o["predicted"] == o["ground_truth"],
            "Cost": o["metrics"].get("cost_usd", 0.0)
        } for o in self.ran_data])
        plt.figure(figsize=(6, 4))
        sns.boxplot(data=df, x="Exact Match", y="Cost")
        plt.title("Cost by Exact Match")
        plt.tight_layout()
        plt.show()






class FovAnalyzer(Analyzer):
    def __init__(self, data: List[Dict[str, Any]], ran_data: List[Dict[str, Any]]):
        # fov のみに絞る
        fov_data = [d for d in data if d["output"]["filter_type"] == "fov"]
        fov_ran = [r for r in ran_data if r["ground_truth"]["filter_type"] == "fov"]
        super().__init__(fov_data, fov_ran)

    def compute_param_accuracy(self) -> float:
        total = len(self.ran_data)
        match = 0
        for o in self.ran_data:
            gt = o["ground_truth"]["params"]
            pred = o["predicted"]["params"]
            if (
                pred.get("isInFov") == gt.get("isInFov") and
                pred.get("order") == gt.get("order") and
                abs(pred.get("range", 0.0) - gt.get("range", 0.0)) < 1e-4
            ):
                match += 1
        return match / total if total else 0.0

    def param_accuracy_by_range(self) -> Dict[float, float]:
        from collections import defaultdict
        grouped = defaultdict(lambda: {"correct": 0, "total": 0})
        for o in self.ran_data:
            gt = o["ground_truth"]["params"]
            pred = o["predicted"]["params"]     
            r = round(gt.get("range", 0.0), 2)
            match = (
                pred.get("isInFov") == gt.get("isInFov") and
                pred.get("order") == gt.get("order") and
                abs(pred.get("range", 0.0) - gt.get("range", 0.0)) < 1e-4
            )
            grouped[r]["total"] += 1
            if match:
                grouped[r]["correct"] += 1
        return {
            r: v["correct"] / v["total"] if v["total"] else 0.0
            for r, v in grouped.items()
        }

    def evaluate_specific(self) -> Dict[str, Any]:
        return {
            "param_accuracy": self.compute_param_accuracy(),
            "param_accuracy_by_range": self.param_accuracy_by_range()
        }
    

    def param_accuracy_breakdown(self) -> Dict[str, float]:
        total = 0
        isInFov_correct = 0
        order_correct = 0
        range_correct = 0

        for o in self.ran_data:
            if o["predicted"]["filter_type"] != "fov":
                continue  # fovと分類されたものだけ対象

            gt = o["ground_truth"]["params"]
            pred = o["predicted"]["params"]

            if pred.get("isInFov") == gt.get("isInFov"):
                isInFov_correct += 1
            if pred.get("order") == gt.get("order"):
                order_correct += 1
            if abs(pred.get("range", 0.0) - gt.get("range", 0.0)) < 1e-4:
                range_correct += 1
            total += 1

        return {
            "isInFov_accuracy": isInFov_correct / total if total else 0.0,
            "order_accuracy": order_correct / total if total else 0.0,
            "range_accuracy": range_correct / total if total else 0.0
        }
    

    
    def compute_param_metrics(self) -> Dict[str, float]:
        # isInFov → binary classification
        isInFov_y_true = []
        isInFov_y_pred = []

        # order → multi-class
        order_y_true = []
        order_y_pred = []

        # range → numerical error
        range_errors = []

        for o in self.ran_data:
            if o["predicted"]["filter_type"] != "fov":
                continue

            gt = o["ground_truth"]["params"]
            pred = o["predicted"]["params"]

            # ✅ None チェックで安全にリストへ追加
            gt_isInFov = gt.get("isInFov")
            pred_isInFov = pred.get("isInFov")
            if gt_isInFov is not None and pred_isInFov is not None:
                isInFov_y_true.append(gt_isInFov)
                isInFov_y_pred.append(pred_isInFov)

            gt_order = gt.get("order")
            pred_order = pred.get("order")
            if gt_order is not None and pred_order is not None:
                order_y_true.append(gt_order)
                order_y_pred.append(pred_order)

            gt_range = gt.get("range")
            pred_range = pred.get("range")
            if gt_range is not None and pred_range is not None:
                range_errors.append(abs(pred_range - gt_range))

        # ✅ sklearnに渡す前に空チェック
        if isInFov_y_true and isInFov_y_pred:
            isInFov_metrics = precision_recall_fscore_support(
                isInFov_y_true, isInFov_y_pred, average="binary", zero_division=0)
        else:
            isInFov_metrics = (0.0, 0.0, 0.0)

        if order_y_true and order_y_pred:
            order_metrics = precision_recall_fscore_support(
                order_y_true, order_y_pred, average="macro", zero_division=0)
        else:
            order_metrics = (0.0, 0.0, 0.0)

        range_mae = np.mean(range_errors) if range_errors else 0.0

        return {
            # isInFov
            "isInFov_precision": isInFov_metrics[0],
            "isInFov_recall": isInFov_metrics[1],
            "isInFov_f1": isInFov_metrics[2],
            # order
            "order_precision": order_metrics[0],
            "order_recall": order_metrics[1],
            "order_f1": order_metrics[2],
            # range
            "range_mae": range_mae
        }

    def plot_param_metrics(self):
        metrics = self.compute_param_metrics()

        df = pd.DataFrame([
            {"Parameter": "isInFov", "Metric": "Precision", "Value": metrics["isInFov_precision"]},
            {"Parameter": "isInFov", "Metric": "Recall", "Value": metrics["isInFov_recall"]},
            {"Parameter": "isInFov", "Metric": "F1", "Value": metrics["isInFov_f1"]},
            {"Parameter": "order", "Metric": "Precision", "Value": metrics["order_precision"]},
            {"Parameter": "order", "Metric": "Recall", "Value": metrics["order_recall"]},
            {"Parameter": "order", "Metric": "F1", "Value": metrics["order_f1"]},
        ])

        plt.figure(figsize=(8, 5))
        sns.barplot(data=df, x="Metric", y="Value", hue="Parameter")
        plt.ylim(0, 1)
        plt.title("Parameter-wise Precision / Recall / F1-score")
        plt.tight_layout()
        plt.show()

        # Range MAE を別で表示
        print(f"Range MAE (Mean Absolute Error): {metrics['range_mae']:.2f} m")



    def show_param_metric_table(self):
        metrics = self.compute_param_metrics()
        df = pd.DataFrame([
            ["isInFov", "Precision", metrics["isInFov_precision"]],
            ["isInFov", "Recall", metrics["isInFov_recall"]],
            ["isInFov", "F1", metrics["isInFov_f1"]],
            ["order", "Precision", metrics["order_precision"]],
            ["order", "Recall", metrics["order_recall"]],
            ["order", "F1", metrics["order_f1"]],
            ["range", "MAE", metrics["range_mae"]],
        ], columns=["Parameter", "Metric", "Value"])
        print(df.to_markdown(index=False))

