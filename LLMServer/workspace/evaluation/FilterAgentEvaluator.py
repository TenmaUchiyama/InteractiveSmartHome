import json
import os
from typing import List, Dict, Any, Optional, Tuple
from dotenv import load_dotenv, set_key
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
from sklearn.metrics import (
    confusion_matrix,
    classification_report,
)
from no_tool_agent_runner import getSystemRunner
from sr_app_types.no_tool_agent_types import State, FilterAgentOutput
from evaluation.Analyzer import Analyzer, FovAnalyzer


# Seaborn のスタイル設定
sns.set(style="whitegrid")

class FilterAgentEvaluator:
    def __init__(self, data_path: str, runner: Optional[Any] = None):
        load_dotenv()  # .env をロード
        try:
            print("=== INIT START ===")
            self.ran_data: List[Dict[str, Any]] = []
            self.data_path = data_path
            self.runner = runner or getSystemRunner()
      
            self._load_data()
        
        except Exception as e:
            print("EXCEPTION IN INIT:", e)

    def _load_data(self):
        """JSONファイルからテストケースを読み込み、self.data に格納"""
        with open(self.data_path, "r", encoding="utf-8") as f:
            self.data: List[Dict[str, Any]] = json.load(f)


    def run_tests(self, start: int = 0, end: Optional[int] = None) -> List[Dict[str, Any]]:
        end = end or len(self.data)
        print(f"🚀 実行範囲: index {start} ～ {end - 1}")

        self.ran_data.clear()

        for idx in range(start, min(end, len(self.data))):
            item = self.data[idx]
            print("=========================")
            print(f"🔍 [{idx}] 実行中: ID = {item['id']} 🗨️「{item['user_prompt']}」")

            state = State(user_prompt=item["user_prompt"])
            res = self.runner.invoke(state)
            fa = res["filterAgent"]

            result = self._run_single_test(item, fa)
            self.ran_data.append(result)

            print("=========================")

        print("\n📊 ✅ 実行結果一覧 (Markdown表形式)")

        
        return self.ran_data


    from sr_app_types.no_tool_agent_types import FilterAgentOutput

    def _run_single_test(self, item: Dict[str, Any], fa: Any) -> Dict[str, Any]:
        pred: FilterAgentOutput = fa.output_data
        gt = item["output"]

        output = {
            "id": item["id"],
            "ground_truth": gt,
            "predicted": {
                "filter_type": pred.filter_type,
                "params": pred.params,
                "reasoning": pred.reasoning
            },
            "metrics": fa.metrics,
            "used_model": os.getenv("GPT_MODEL"),
        }

        params_pred = pred.params or {}
        params_gt = gt.get("params", {})

        exact = pred.filter_type == gt.get("filter_type") and params_pred == params_gt
        ft_match = pred.filter_type == gt.get("filter_type")
        isInFov_match = params_pred.get("isInFov") == params_gt.get("isInFov")
        order_match = params_pred.get("order") == params_gt.get("order")
        range_match = abs(params_pred.get("range", 0.0) - params_gt.get("range", 0.0)) < 1e-4

        emoji_result = "✅" if exact else ("🔸" if ft_match else "❌")

        print(f"   結果: {emoji_result}  filter_type: {'✅' if ft_match else '❌'} | "
            f"isInFov: {'✅' if isInFov_match else '❌'}, "
            f"order: {'✅' if order_match else '❌'}, "
            f"range: {'✅' if range_match else '❌'}"
            )
        if fa.metrics:
            print("time_taken: ", fa.metrics.get("elapsed_seconds"))
            print("cost_usd: $", fa.metrics.get("cost_usd"))

        return output

    
    def save_outputs(self, path: str):
        """run_tests の outputs を JSON 形式で保存"""
        os.makedirs(os.path.dirname(path), exist_ok=True)
        with open(path, "w", encoding="utf-8") as f:
            json.dump(self.ran_data, f, ensure_ascii=False, indent=2)

    @property
    def analyzer(self) -> Analyzer:
        return Analyzer(data= self.data, ran_data= self.ran_data)

    def type_analyzer(self, label: str) -> Analyzer:
        if label == "fov":
            return FovAnalyzer(data=self.data, ran_data=self.ran_data)
        elif label == "direction":
            return FovAnalyzer(data=self.data, ran_data=self.ran_data)
        elif label == "around_furniture":
            return FovAnalyzer(data=self.data, ran_data=self.ran_data)
        else:
            return FovAnalyzer(data=self.data, ran_data=self.ran_data)
    
    def change_model(self, model: str):
        ENV_PATH = "../.env"
        set_key(ENV_PATH, "GPT_MODEL", model)
        os.environ["GPT_MODEL"] = model
        load_dotenv(ENV_PATH, override=True)
        print(f"[change_model] GPT_MODEL set to: {model}")

    def getDataById(self, target_id: str) -> Dict[str, Any]:
        test_data = next((item for item in self.data if item["id"] == target_id), None)
        result_data = next((item for item in self.ran_data if item["id"] == target_id), None)
        return {"test_data": test_data, "result_data": result_data}

    def write_output_summary(self, output_dir: str):
        """
        全体のモデル出力と評価結果を保存する（summary/ 下に格納）：
        - {output_dir}/summary/json/ran_data.json
        - {output_dir}/summary/json/summary.json
        - {output_dir}/summary/csv/summary.csv
        """
        summary_dir = os.path.join(output_dir, "summary", os.getenv("GPT_MODEL"))
        json_dir = os.path.join(summary_dir, "json", os.getenv("GPT_MODEL"))
        csv_dir = os.path.join(summary_dir, "csv", os.getenv("GPT_MODEL"))
        os.makedirs(json_dir, exist_ok=True)
        os.makedirs(csv_dir, exist_ok=True)

        # 🔹 1. ran_data の保存
        ran_data_path = os.path.join(json_dir, "ran_data.json")
        with open(ran_data_path, "w", encoding="utf-8") as f:
            json.dump(self.ran_data, f, ensure_ascii=False, indent=2)
        print(f"✅ ran_data saved to: {ran_data_path}")

        # 🔹 2. summary の保存（JSON形式）
        analyzer = self.analyzer
        summary = analyzer.evaluate_all(labels=["fov", "direction", "around_furniture", "all"])
        summary["note"] = "この結果は filter_type に基づいた分類性能の評価を含みます。"

        if isinstance(summary.get("confusion_matrix"), np.ndarray):
            summary["confusion_matrix"] = summary["confusion_matrix"].tolist()

        summary_json_path = os.path.join(json_dir, "summary.json")
        with open(summary_json_path, "w", encoding="utf-8") as f:
            json.dump(summary, f, ensure_ascii=False, indent=2)
        print(f"📊 summary saved to: {summary_json_path}")

        # 🔹 3. summary の保存（CSV形式）
        summary_rows = []
        for key, value in summary.items():
            if isinstance(value, dict):
                for subkey, subval in value.items():
                    summary_rows.append({"Metric": f"{key}.{subkey}", "Value": subval})
            elif isinstance(value, list):
                summary_rows.append({"Metric": key, "Value": json.dumps(value, ensure_ascii=False)})
            else:
                summary_rows.append({"Metric": key, "Value": value})

        summary_df = pd.DataFrame(summary_rows)
        summary_csv_path = os.path.join(csv_dir, "summary.csv")
        summary_df.to_csv(summary_csv_path, index=False)


        task_table = analyzer.display_overview()
        task_table_path = os.path.join(csv_dir, "overview.md")
        task_table.to_markdown(task_table_path, index=False)
        print(f"📁 summary CSV saved to: {summary_csv_path}")

    def write_type_summaries(self, output_dir: str, types: List[str] = ["fov", "direction", "around_furniture", "all"]):
        """
        filter_typeごとの summary を JSON/CSV で保存する
        - output_dir/{type}/summary.json
        - output_dir/{type}/summary.csv
        """
        for t in types:
            print(f"📁 generating output for filter_type: {t}")
            analyzer = self.type_analyzer(t)
            summary = analyzer.evaluate_all(labels=types)
            summary["note"] = f"filter_type = '{t}' に関する性能評価"

            # ndarray などを list に変換
            if isinstance(summary.get("confusion_matrix"), np.ndarray):
                summary["confusion_matrix"] = summary["confusion_matrix"].tolist()

            # --- 出力ディレクトリ準備 ---
            type_dir = os.path.join(output_dir, t)
            os.makedirs(type_dir, exist_ok=True)

            # --- JSON保存 ---
            json_path = os.path.join(type_dir, "summary.json")
            with open(json_path, "w", encoding="utf-8") as f:
                json.dump(summary, f, ensure_ascii=False, indent=2)
            print(f"✅ JSON written to: {json_path}")

            # --- CSV保存 ---
            rows = []
            for key, value in summary.items():
                if isinstance(value, dict):
                    for subkey, subval in value.items():
                        rows.append({"Metric": f"{key}.{subkey}", "Value": subval})
                elif isinstance(value, list):
                    rows.append({"Metric": key, "Value": json.dumps(value, ensure_ascii=False)})
                else:
                    rows.append({"Metric": key, "Value": value})

            df = pd.DataFrame(rows)
            csv_path = os.path.join(type_dir, "summary.csv")
            df.to_csv(csv_path, index=False)
            print(f"📁 CSV written to: {csv_path}")
