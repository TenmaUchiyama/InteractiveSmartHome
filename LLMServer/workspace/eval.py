from dotenv import load_dotenv
load_dotenv("../.env")
import multiprocessing
import os
from evaluation.FilterAgentEvaluator import FilterAgentEvaluator
from no_tool_agent_runner import getFilterDeviceRunner


def run_for_model(model_name: str, data_path: str, output_root: str):
    print(f"[{model_name}] 開始")
    runner = getFilterDeviceRunner()
    evaluator = FilterAgentEvaluator(data_path, runner)
    evaluator.change_model(model_name)
    evaluator.run_tests(0, len(evaluator.data))
    output_dir = os.path.join(output_root, model_name)
    evaluator.write_output_summary(output_dir)
    evaluator.write_type_summaries(output_dir)
    print(f"[{model_name}] 完了 ✅")


def main():
    models = [
        "gpt-4.1-nano",
        "gpt-4o",
        "gpt-4.1",
    ]
    data_path = "../Evaluation/TestData/en/long/fov.json"
    output_root = "../Evaluation/Result"

    # Windows対応：プロセス保護
    with multiprocessing.Pool(processes=len(models)) as pool:
        pool.starmap(run_for_model, [(model, data_path, output_root) for model in models])


if __name__ == "__main__":
    multiprocessing.freeze_support()  # Windowsで必要なことがある
    main()
