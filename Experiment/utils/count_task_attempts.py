import json
import os
from pathlib import Path
import pandas as pd

def count_attempts_in_file(file_path):
    """
    単一のJSONファイルのtaskAttemptsの個数を数える
    """
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            data = json.load(f)
        
        if not isinstance(data, list):
            return None
        
        total_attempts = 0
        task_details = []
        
        for i, task in enumerate(data):
            if isinstance(task, dict) and 'taskAttempts' in task:
                attempts = task['taskAttempts']
                if isinstance(attempts, list):
                    attempt_count = len(attempts)
                    total_attempts += attempt_count
                    
                    # タスクの詳細情報を記録
                    task_info = {
                        'file': file_path.name,
                        'task_index': i,
                        'task_id': task.get('taskId', 'N/A'),
                        'attempt_count': attempt_count,
                        'task_attempt_count': task.get('taskAttemptCount', 0),
                        'user_command': attempts[0].get('userCommand', 'N/A') if attempts else 'N/A'
                    }
                    task_details.append(task_info)
        
        return {
            'file': file_path.name,
            'total_attempts': total_attempts,
            'task_count': len([task for task in data if isinstance(task, dict) and 'taskAttempts' in task]),
            'task_details': task_details
        }
        
    except Exception as e:
        print(f"エラー: {file_path} - {str(e)}")
        return None

def analyze_all_json_files(directory_path):
    """
    指定されたディレクトリ内のすべてのJSONファイルを分析する
    """
    directory = Path(directory_path)
    json_files = list(directory.rglob("*.json"))
    
    print(f"分析対象ファイル数: {len(json_files)}")
    print("=" * 50)
    
    all_results = []
    all_task_details = []
    
    for json_file in json_files:
        if json_file.name.endswith('.json') and not json_file.name.endswith('.meta'):
            result = count_attempts_in_file(json_file)
            if result:
                all_results.append(result)
                all_task_details.extend(result['task_details'])
                print(f"ファイル: {result['file']}")
                print(f"  タスク数: {result['task_count']}")
                print(f"  総attempt数: {result['total_attempts']}")
                print("-" * 30)
    
    # 結果をDataFrameに変換
    if all_results:
        df_summary = pd.DataFrame(all_results)
        df_details = pd.DataFrame(all_task_details)
        
        # 統計情報を表示
        print("=" * 50)
        print("統計情報:")
        print(f"総ファイル数: {len(df_summary)}")
        print(f"総タスク数: {df_summary['task_count'].sum()}")
        print(f"総attempt数: {df_summary['total_attempts'].sum()}")
        print(f"平均attempt数/ファイル: {df_summary['total_attempts'].mean():.2f}")
        print(f"最大attempt数/ファイル: {df_summary['total_attempts'].max()}")
        print(f"最小attempt数/ファイル: {df_summary['total_attempts'].min()}")
        
        # 詳細な分析
        print("\n詳細分析:")
        print(f"平均attempt数/タスク: {df_details['attempt_count'].mean():.2f}")
        print(f"最大attempt数/タスク: {df_details['attempt_count'].max()}")
        print(f"最小attempt数/タスク: {df_details['attempt_count'].min()}")
        
        # attempt数が多いタスクを表示
        high_attempt_tasks = df_details[df_details['attempt_count'] > 1].sort_values('attempt_count', ascending=False)
        if not high_attempt_tasks.empty:
            print(f"\n複数attemptがあるタスク数: {len(high_attempt_tasks)}")
            print("上位5件:")
            for _, task in high_attempt_tasks.head().iterrows():
                print(f"  ファイル: {task['file']}, タスクID: {task['task_id']}, attempt数: {task['attempt_count']}")
                print(f"    コマンド: {task['user_command'][:50]}...")
        
        # CSVファイルに保存
        timestamp = pd.Timestamp.now().strftime("%Y%m%d_%H%M%S")
        summary_file = f"attempt_analysis_summary_{timestamp}.csv"
        details_file = f"attempt_analysis_details_{timestamp}.csv"
        
        df_summary.to_csv(summary_file, index=False, encoding='utf-8-sig')
        df_details.to_csv(details_file, index=False, encoding='utf-8-sig')
        
        print(f"\n結果を保存しました:")
        print(f"  サマリー: {summary_file}")
        print(f"  詳細: {details_file}")
        
        return df_summary, df_details
    
    return None, None

if __name__ == "__main__":
    # VOICE_LOGディレクトリのパス
    voice_log_dir = "../../InteractiveSmartHome/Assets/EXPERIMENT/VOICE_LOG"
    
    # すべてのJSONファイルを分析
    analyze_all_json_files(voice_log_dir) 