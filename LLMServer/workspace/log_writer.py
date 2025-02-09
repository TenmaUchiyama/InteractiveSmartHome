import os
import json
import dotenv
from fastapi import HTTPException

dotenv.load_dotenv()

# 環境変数の取得
ROOT_PATH = os.getenv("PROJECT_PATH", ".")  # デフォルト値としてカレントディレクトリを指定
USER_NAME = os.getenv("USER_NAME", "default_user")  # デフォルト値
LOG_DIR = os.path.join(ROOT_PATH, "log", USER_NAME)

# ログディレクトリを作成
os.makedirs(LOG_DIR, exist_ok=True)




def add_label_log(task_id, data):
    add_log("Label", task_id, data)

def add_pointing_log(task_id, data):
    add_log("Pointing", task_id, data)

def add_llm_log(task_id, data):
    add_log("Spatial", task_id, data)







def add_log(CONDITION , task_id, data):
    sending_data = {
        "task_id": task_id,
        "llm_output": _serialize_data(data),  # JSON変換できる形にする
        "user_name": USER_NAME
    }

    # Label ディレクトリを作成
    LOG_LABEL_DIR = os.path.join(LOG_DIR, CONDITION)
    os.makedirs(LOG_LABEL_DIR, exist_ok=True)

    # ファイルパスを決定（拡張子を .json に変更）
    LOG_FILE_PATH = os.path.join(LOG_LABEL_DIR, f"{USER_NAME}_{sending_data['task_id']}.json")
    """ログをJSONファイルに書き込む関数"""
    os.makedirs(os.path.dirname(LOG_FILE_PATH), exist_ok=True)  # 必要なディレクトリを作成
    with open(LOG_FILE_PATH, "w", encoding="utf-8") as f:
        json.dump(sending_data, f, ensure_ascii=False, indent=4)
    return LOG_FILE_PATH


def _serialize_data(data):
    """JSON に変換できないオブジェクトを処理"""
    try:
        return json.dumps(data, ensure_ascii=False)  # そのまま JSON 変換を試す
    except TypeError:
        if isinstance(data, dict):
            return {key: _serialize_data(value) for key, value in data.items()}  # 再帰的に処理
        elif isinstance(data, list):
            return [_serialize_data(item) for item in data]  # リストの場合も処理
        else:
            return str(data)  # 文字列に変換
