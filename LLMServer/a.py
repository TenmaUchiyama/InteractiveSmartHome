import json
import re
from pathlib import Path

# ファイルパス
arrangement_path = Path(r"C:\Users\tenma\Desktop\KeioSchool\InteractiveSmartHome\InteractiveSmartHome\InteractiveSmartHome\Assets\EXPERIMENT\ArrangeData\PreTaskArrangement.json")
ids_path = Path(r"C:\Users\tenma\AppData\LocalLow\DefaultCompany\InteractiveSmartHome\device_ids.json")

# 英語名から日本語名への正規化関数
def normalize_jp_name(en_name):
    en_name = en_name.replace("　", " ")  # 全角スペース補正
    num_match = re.search(r'\d+', en_name)
    num = num_match.group() if num_match else ""

    if "Ceiling Light" in en_name:
        return f"天井ライト{num}"
    elif "TV Light" in en_name:
        return f"テレビライト{num}"
    elif "Floor Light" in en_name:
        return f"フロアライト{num}"
    elif "Wall Light" in en_name:
        return f"壁ランプ{num}"
    elif "Shelf Light" in en_name:
        return f"棚ライト{num}"
    elif "Stand Light" in en_name:
        return f"ランプスタンド{num}"
    elif "Table Light" in en_name or "Table" in en_name:
        return f"テーブルライト{num}"
    elif "Plant Light" in en_name:
        return f"植物ライト{num}"
    else:
        return None

# ファイル読み込み
with open(arrangement_path, "r", encoding="utf-8") as f:
    arrangement_data = json.load(f)

with open(ids_path, "r", encoding="utf-8") as f:
    id_data = json.load(f)

# デバイス名マップの構築（日本語名 → 英語名・ID）
jp_to_en = {}
english_device_list = id_data["devices"]

for item in english_device_list:
    en_name = item["deviceName"]
    en_id = item["deviceId"]
    jp_name = normalize_jp_name(en_name)
    if jp_name:
        jp_to_en[jp_name] = {
            "deviceName": en_name,
            "deviceId": en_id
        }

# 補完ルール（番号なし用 fallback）
fallback_map = {
    "テーブルライト": "Table Light",
    "フロアライト": "Floor Light",
    "ランプスタンド": "Stand Light"
}

for jp_base, en_base in fallback_map.items():
    for item in english_device_list:
        en_name = item["deviceName"]
        if en_name.startswith(en_base):
            en_id = item["deviceId"]
            if jp_base not in jp_to_en:
                jp_to_en[jp_base] = {
                    "deviceName": en_name,
                    "deviceId": en_id
                }
            break  # 最初の一致のみ使用

# 数字正規化関数（全角→半角）
def normalize_digits(text):
    full_to_half = str.maketrans("０１２３４５６７８９", "0123456789")
    return text.translate(full_to_half).strip()

# 変換処理
if isinstance(arrangement_data, list):
    for entry in arrangement_data:
        new_devices = []
        for device in entry.get("devices", []):
            jp_name_raw = device.get("deviceName", "")
            jp_name = normalize_digits(jp_name_raw)
            color = device.get("colorName", "")

            if jp_name in jp_to_en:
                matched = jp_to_en[jp_name]
                new_devices.append({
                    "deviceName": matched["deviceName"],
                    "colorName": color,
                    "deviceId": matched["deviceId"]
                })
            else:
                print(f"[警告] 対応する英語名が見つかりません: {jp_name_raw}")
        entry["devices"] = new_devices
else:
    print("[エラー] arrangement_data は list ではありません")

# 保存
output_path = arrangement_path.with_name("PreTaskArrangement_english.json")
with open(output_path, "w", encoding="utf-8") as f:
    json.dump(arrangement_data, f, indent=2, ensure_ascii=False)

print(f"[完了] 英語デバイス名に変換されたファイルを保存しました: {output_path}")
