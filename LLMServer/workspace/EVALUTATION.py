from dotenv import load_dotenv
load_dotenv("../.env")

import json
import os
from no_tool_agent_runner import getFilterDeviceRunner 
from sr_app_types.no_tool_agent_types import State

# === 設定 ===
FOV_PATH = "../Evaluation/TestData/en/short/fov.json"
RESULT_DIR = "Evaluation/Result"
RESULT_FILE = os.path.join(RESULT_DIR, "fov_test_results.json")
MAX_TESTS = 10

# === データ読み込み ===
with open(FOV_PATH, "r", encoding="utf-8") as f:
    fov_data = json.load(f)

# === 結果保存ディレクトリ作成 ===
os.makedirs(RESULT_DIR, exist_ok=True)

# === 初期化 ===
runner = getFilterDeviceRunner()
results = []
test_count = 0
success_count = 0

# === テスト実行 ===
# === テスト実行 ===
for i, item in enumerate(fov_data[:MAX_TESTS]):
    user_prompt = item["user_prompt"]
    expected_filter_type = item["output"]["filter_type"]
    expected_params = item["output"]["params"]

    print(f"\n===== FOVテスト {i+1}/{MAX_TESTS} =====")
    print(f"🗣 user_prompt: {user_prompt}")

    # Agent 実行
    state = State(user_prompt=user_prompt)
    res = runner.invoke(state)
    selected_tool = res['filterAgent'].selected_tool
    actual_filter_type = selected_tool.get("filter_type", None)
    actual_params = selected_tool.get("params", {})

    # 評価
    type_match = (expected_filter_type == actual_filter_type)
    params_match = (expected_params == actual_params)
    overall_match = type_match and params_match

    test_count += 1
    if overall_match:
        success_count += 1

    print(f"✅ expected: {expected_filter_type}, {expected_params}")
    print(f"🏁 actual:   {actual_filter_type}, {actual_params}")
    print(f"🎯 filter_type一致: {type_match}")
    print(f"🎯 params一致:      {params_match}")
    print(f"🎯 総合評価: {'成功 ✓' if overall_match else '失敗 ✗'}")

    # メトリクス取得
    token_info = callback.last_tokens
    cost_info = callback.last_cost
    time_info = callback.last_time

    # 結果保存
    results.append({
        "index": i,
        "user_prompt": user_prompt,
        "expected": {
            "filter_type": expected_filter_type,
            "params": expected_params
        },
        "actual": {
            "filter_type": actual_filter_type,
            "params": actual_params
        },
        "evaluation": {
            "filter_type_match": type_match,
            "params_match": params_match,
            "overall_match": overall_match
        },
        "metrics": {
            "tokens": token_info,
            "cost_usd": cost_info,
            "elapsed_seconds": time_info
        }
    })

    break


