import json
import pandas as pd
df = pd.read_csv("latin_square_design.csv")
index = 0
p1 = df.iloc[0]


#研究被験者の取得
participant = f"P{index+7}"
taskSet = p1["Task Set 1"]
condition_num = int(p1["Condition 1_ConditionNum"])




#ArrangeDataを取得する
with open(f"../InteractiveSmartHome/Assets/EXPERIMENT/ArrangeData/PreTaskArrangement{taskSet}.json", "r", encoding="utf-8") as f:
    arrangeData = json.loads(f.read())

with open(f"../InteractiveSmartHome/Assets/EXPERIMENT/VOICE_LOG/{participant}/{condition_num}_{taskSet}.json", "r", encoding="utf-8") as f:
    voice_log = json.loads(f.read())
    
