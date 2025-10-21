import json
import shutil
from pathlib import Path
from datetime import datetime


def remove_duplicate_attempts(data):
    """
    taskAttempts配列からattemptIdが重複しているエントリを除去する
    - 同じattemptIdが複数回現れる場合は最後の出現（最新）を採用する
    - taskAttemptCountを正しい長さに更新する
    - finalIdを最後のattemptのattemptIdに更新する
    """
    if not isinstance(data, list):
        return data

    for task in data:
        if isinstance(task, dict) and 'taskAttempts' in task:
            attempts = task['taskAttempts']
            if isinstance(attempts, list):
                # Collect last occurrence index and object for each attemptId
                last_index = {}
                last_attempt_by_id = {}

                for idx, attempt in enumerate(attempts):
                    if isinstance(attempt, dict) and 'attemptId' in attempt:
                        aid = attempt['attemptId']
                        last_index[aid] = idx
                        last_attempt_by_id[aid] = attempt

                # Reconstruct list ordered by the last occurrence
                sorted_ids = sorted(last_index.items(), key=lambda x: x[1])
                unique_attempts = [last_attempt_by_id[aid] for aid, _ in sorted_ids]

                # Replace with deduped attempts
                task['taskAttempts'] = unique_attempts
                # Update taskAttemptCount
                task['taskAttemptCount'] = len(unique_attempts)

                # Update finalId to the last attempt's id if present
                if unique_attempts:
                    task['finalId'] = unique_attempts[-1].get('attemptId')

    return data


def backup_file(file_path, backup_dir):
    """
    ファイルをバックアップディレクトリにコピーする
    """
    try:
        backup_dir.mkdir(parents=True, exist_ok=True)

        # Preserve the same filename under the backup directory
        backup_path = backup_dir / file_path.name

        shutil.copy2(file_path, backup_path)
        print(f"backup created: {backup_path}")
        return True

    except Exception as e:
        print(f"backup error: {file_path} - {str(e)}")
        return False


def process_json_file(file_path, backup_dir):
    """
    単一のJSONファイルを処理する
    """
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            data = json.load(f)

        original_count = sum(len(task.get('taskAttempts', [])) for task in data if isinstance(task, dict))

        # 重複除去処理
        processed_data = remove_duplicate_attempts(data)

        new_count = sum(len(task.get('taskAttempts', [])) for task in processed_data if isinstance(task, dict))

        # 変更があった場合のみファイルを更新
        if original_count != new_count:
            # バックアップを作成
            backup_file(file_path, backup_dir)

            # ファイルを更新
            with open(file_path, 'w', encoding='utf-8') as f:
                json.dump(processed_data, f, ensure_ascii=False, indent=2)

            print(f"処理完了: {file_path}")
            print(f"  元のattempt数: {original_count}")
            print(f"  新しいattempt数: {new_count}")
            print(f"  削除された重複: {original_count - new_count}")
            return True
        else:
            print(f"変更なし: {file_path}")
            return False

    except Exception as e:
        print(f"エラー: {file_path} - {str(e)}")
        return False


def process_all_json_files(directory_path):
    """
    指定されたディレクトリ内のすべてのJSONファイルを処理する
    """
    directory = Path(directory_path)
    json_files = list(directory.rglob("*.json"))

    # backup dir inside the provided directory
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    backup_dir = directory / f"backup_{timestamp}"

    print(f"processing files: {len(json_files)}")
    print(f"backup dir: {backup_dir}")
    print("=" * 50)

    processed_count = 0
    modified_count = 0

    for json_file in json_files:
        # skip metadata files
        if json_file.name.endswith('.json') and not json_file.name.endswith('.meta'):
            processed_count += 1
            if process_json_file(json_file, backup_dir):
                modified_count += 1
            print("-" * 30)

    print("=" * 50)
    print(f"done: processed {processed_count} files, fixed {modified_count} files")
    print(f"backups saved under {backup_dir}")


if __name__ == "__main__":
    # Use the VOICE_LOG directory inside the Experiment folder by default
    voice_log_dir = Path(__file__).resolve().parents[1] / "NumericalAnalyzsis" / "VOICE_LOG"
    if not voice_log_dir.exists():
        # fallback to a relative path if structure differs
        voice_log_dir = Path("./NumericalAnalyzsis/VOICE_LOG")

    process_all_json_files(str(voice_log_dir))