import json
import os
import shutil
from pathlib import Path
from datetime import datetime

def remove_duplicate_attempts(data):
    """
    taskAttempts配列からattemptIdが重複しているエントリを除去する
    """
    if not isinstance(data, list):
        return data
    
    for task in data:
        if isinstance(task, dict) and 'taskAttempts' in task:
            attempts = task['taskAttempts']
            if isinstance(attempts, list):
                # attemptIdでユニークにする
                seen_attempt_ids = set()
                unique_attempts = []
                
                for attempt in attempts:
                    if isinstance(attempt, dict) and 'attemptId' in attempt:
                        attempt_id = attempt['attemptId']
                        if attempt_id not in seen_attempt_ids:
                            seen_attempt_ids.add(attempt_id)
                            unique_attempts.append(attempt)
                
                # 重複を除去した配列で置き換え
                task['taskAttempts'] = unique_attempts
                # taskAttemptCountも更新
                task['taskAttemptCount'] = len(unique_attempts)
    
    return data

def backup_file(file_path, backup_dir):
    """
    ファイルをバックアップディレクトリにコピーする
    """
    try:
        # バックアップディレクトリが存在しない場合は作成
        backup_dir.mkdir(parents=True, exist_ok=True)
        
        # 元のファイルの相対パスを保持
        relative_path = file_path.relative_to(Path("../../InteractiveSmartHome/Assets/EXPERIMENT/VOICE_LOG"))
        backup_path = backup_dir / relative_path
        
        # バックアップ先のディレクトリを作成
        backup_path.parent.mkdir(parents=True, exist_ok=True)
        
        # ファイルをコピー
        shutil.copy2(file_path, backup_path)
        print(f"バックアップ作成: {backup_path}")
        return True
        
    except Exception as e:
        print(f"バックアップエラー: {file_path} - {str(e)}")
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
    
    # バックアップディレクトリを作成（タイムスタンプ付き）
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    backup_dir = Path("../../InteractiveSmartHome/Assets/EXPERIMENT/OLD") / f"backup_{timestamp}"
    
    print(f"処理対象ファイル数: {len(json_files)}")
    print(f"バックアップディレクトリ: {backup_dir}")
    print("=" * 50)
    
    processed_count = 0
    modified_count = 0
    
    for json_file in json_files:
        if json_file.name.endswith('.json') and not json_file.name.endswith('.meta'):
            processed_count += 1
            if process_json_file(json_file, backup_dir):
                modified_count += 1
            print("-" * 30)
    
    print("=" * 50)
    print(f"処理完了: {processed_count}ファイル中{modified_count}ファイルを修正しました")
    print(f"バックアップは {backup_dir} に保存されました")

if __name__ == "__main__":
    # VOICE_LOGディレクトリのパス
    voice_log_dir = "../../InteractiveSmartHome/Assets/EXPERIMENT/VOICE_LOG"
    
    # すべてのJSONファイルを処理    
    process_all_json_files(voice_log_dir) 