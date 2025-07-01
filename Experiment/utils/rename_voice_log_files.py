import os
import shutil
from pathlib import Path

def rename_voice_log_files():
    """
    VOICE_LOGディレクトリ内のファイル名を
    {condition_num}_{taskSet}.json から {participant}_{condition_num}.json に変更する
    """
    
    # VOICE_LOGディレクトリのパス
    voice_log_dir = Path("../../InteractiveSmartHome/Assets/EXPERIMENT/VOICE_LOG")
    
    # 各参加者ディレクトリを処理
    for participant_dir in voice_log_dir.iterdir():
        if not participant_dir.is_dir() or participant_dir.name == "OLD":
            continue
            
        participant = participant_dir.name
        print(f"処理中: {participant}")
        
        # ディレクトリ内の.jsonファイルを処理
        for json_file in participant_dir.glob("*.json"):
            if json_file.name.endswith('.json'):
                # ファイル名を_でスプリット
                parts = json_file.stem.split('_')
                if len(parts) == 2:
                    condition_num = parts[0]
                    task_set = parts[1]
                    
                    # 新しいファイル名
                    new_filename = f"{participant}_{condition_num}.json"
                    new_filepath = participant_dir / new_filename
                    
                    try:
                        # ファイル名を変更
                        json_file.rename(new_filepath)
                        print(f"  変更: {json_file.name} → {new_filename}")
                        
                        # .metaファイルも存在する場合は変更
                        meta_file = participant_dir / f"{json_file.name}.meta"
                        if meta_file.exists():
                            new_meta_filepath = participant_dir / f"{new_filename}.meta"
                            meta_file.rename(new_meta_filepath)
                            print(f"  変更: {json_file.name}.meta → {new_filename}.meta")
                            
                    except Exception as e:
                        print(f"  エラー: {json_file.name}の変更に失敗しました - {e}")

if __name__ == "__main__":
    print("VOICE_LOGファイル名変更を開始します...")
    rename_voice_log_files()
    print("完了しました！") 