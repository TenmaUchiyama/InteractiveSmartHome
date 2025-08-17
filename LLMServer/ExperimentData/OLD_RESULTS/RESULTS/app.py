import json
import os
import shutil
from pathlib import Path

def split_p17_data():
    """
    P17のデータを3人分（P17, P18, P19）に分割する
    各ファイルの構造：
    - 最初の12エントリ → P17に残す
    - 次の12エントリ → P18に移動  
    - 残りのエントリ → P19に移動
    """
    
    # パス設定
    base_dir = Path(r'.')
    p17_dir = base_dir / 'P17'
    p18_dir = base_dir / 'P18'
    p19_dir = base_dir / 'P19'
    
    # P18とP19のディレクトリを作成
    p18_dir.mkdir(exist_ok=True)
    p19_dir.mkdir(exist_ok=True)
    
    # P17のファイル一覧
    p17_files = ['P17_1.json', 'P17_2.json', 'P17_3.json', 'P17_4.json']
    
    print("データ分割を開始します...")
    
    for file_name in p17_files:
        print(f"\n処理中: {file_name}")
        
        # 元ファイルを読み込み
        p17_file_path = p17_dir / file_name
        with open(p17_file_path, 'r', encoding='utf-8') as f:
            original_data = json.load(f)
        
        total_entries = len(original_data)
        print(f"  元のエントリ数: {total_entries}")
        
        # データを分割
        # P17: 最初の12エントリ
        p17_data = original_data[:12]
        
        # P18: 次の12エントリ（12-23）
        p18_data = original_data[12:24] if total_entries > 12 else []
        
        # P19: 残りのエントリ（24以降）
        p19_data = original_data[24:] if total_entries > 24 else []
        
        print(f"  P17に残すエントリ数: {len(p17_data)}")
        print(f"  P18に移動するエントリ数: {len(p18_data)}")
        print(f"  P19に移動するエントリ数: {len(p19_data)}")
        
        # P17ファイルを更新（最初の12エントリのみ）
        with open(p17_file_path, 'w', encoding='utf-8') as f:
            json.dump(p17_data, f, ensure_ascii=False, indent=2)
        
        # P18ファイルを作成
        if p18_data:
            p18_file_name = file_name.replace('P17_', 'P18_')
            p18_file_path = p18_dir / p18_file_name
            with open(p18_file_path, 'w', encoding='utf-8') as f:
                json.dump(p18_data, f, ensure_ascii=False, indent=2)
            print(f"  作成: {p18_file_path}")
        
        # P19ファイルを作成
        if p19_data:
            p19_file_name = file_name.replace('P17_', 'P19_')
            p19_file_path = p19_dir / p19_file_name
            with open(p19_file_path, 'w', encoding='utf-8') as f:
                json.dump(p19_data, f, ensure_ascii=False, indent=2)
            print(f"  作成: {p19_file_path}")
    
    print("\n分割完了！")
    
    # 結果確認
    print("\n=== 分割結果確認 ===")
    for participant in ['P17', 'P18', 'P19']:
        participant_dir = base_dir / participant
        if participant_dir.exists():
            total_entries = 0
            files_info = []
            
            for file_path in sorted(participant_dir.glob(f'{participant}_*.json')):
                with open(file_path, 'r', encoding='utf-8') as f:
                    data = json.load(f)
                entries_count = len(data)
                total_entries += entries_count
                files_info.append(f"  {file_path.name}: {entries_count}エントリ")
            
            print(f"\n{participant}:")
            for info in files_info:
                print(info)
            print(f"  合計: {total_entries}エントリ")
        else:
            print(f"\n{participant}: フォルダが存在しません")

def backup_original_p17():
    """
    元のP17データをバックアップする
    """
    base_dir = Path(r'c:\Users\tenma\Desktop\KeioSchool\InteractiveSmartHome\InteractiveSmartHome\LLMServer\ExperimentData\RESULTS')
    p17_dir = base_dir / 'P17'
    backup_dir = base_dir / 'P17_BACKUP'
    
    if backup_dir.exists():
        print("バックアップフォルダは既に存在します。")
        return
    
    print("元のP17データをバックアップしています...")
    shutil.copytree(p17_dir, backup_dir)
    print(f"バックアップ完了: {backup_dir}")

if __name__ == "__main__":
    print("P17データ分割ツール")
    print("=" * 50)
    
    # バックアップ作成の確認
    backup_response = input("元のP17データをバックアップしますか？ (y/n): ").lower().strip()
    if backup_response == 'y':
        backup_original_p17()
    
    # 分割実行の確認
    split_response = input("\nデータ分割を実行しますか？ (y/n): ").lower().strip()
    if split_response == 'y':
        split_p17_data()
    else:
        print("分割がキャンセルされました。")