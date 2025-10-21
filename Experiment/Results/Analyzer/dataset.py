# --- dataset.py ---
from __future__ import annotations
from typing import Dict, Iterable, List, Literal, TypedDict
import pandas as pd

from scales import SUSScorer, TAMScorer, TPAScorer, NASATLXScorer

ConditionName = Literal["SpatialReference", "SpatialReference+Pointing", "Pointing", "Label"]

class RowRecord(TypedDict):
    ConditionId: int
    Condition: ConditionName
    SUS: List[float]
    TAM: List[float]
    TPA: List[float]
    NASA_TLX: List[float]

class SurveyDataset:
    """
    1被験者=4行（各行が条件）を前提。
    Conditionは列index=2（1..4）。
    下記COLSのインデックス範囲に各回答が入っている想定。
    """
    COLS = {
        "SUS": list(range(3, 13)),        # 10項目 (1-5)
        "TAM": list(range(13, 25)),       # 12項目 (1-7)
        "TPA": list(range(25, 37)),       # 12項目 (1-7; 後半5がネガ)
        "NASA_TLX": list(range(37, 43)),  # 6項目 (1-21; Performance反転)
    }
    CONDITION_MAP: Dict[int, ConditionName] = {
        1: "SpatialReference",
        2: "SpatialReference+Pointing",
        3: "Pointing",
        4: "Label",
    }

    def __init__(self, df: pd.DataFrame):
        self.df = df.reset_index(drop=True)
        self.rows: List[RowRecord] = []
        for i in range(len(self.df)):
            row = self.df.iloc[i]
            cond_id = int(row.iloc[2])
            self.rows.append({
                "ConditionId": cond_id,
                "Condition": self.CONDITION_MAP.get(cond_id, "Label"),  # 保険
                "SUS":       row.iloc[self.COLS["SUS"]].astype(float).to_list(),
                "TAM":       row.iloc[self.COLS["TAM"]].astype(float).to_list(),
                "TPA":       row.iloc[self.COLS["TPA"]].astype(float).to_list(),
                "NASA_TLX":  row.iloc[self.COLS["NASA_TLX"]].astype(float).to_list(),
            })

    @property
    def condition_order(self) -> List[ConditionName]:
        return [self.CONDITION_MAP[k] for k in sorted(self.CONDITION_MAP.keys())]

    # 記述統計に使うロング形式（箱ひげやFriedmanに直結）
    def to_long_scores(self) -> pd.DataFrame:
        recs = []
        for r in self.rows:
            cond = r["Condition"]
            # SUS
            sus = SUSScorer.score(r["SUS"])
            recs.append({"Condition": cond, "Scale": "SUS", "Subscale": "Total", "Score": sus})

            # TPA
            tpa = TPAScorer.score(r["TPA"])
            recs.append({"Condition": cond, "Scale": "TPA", "Subscale": "Total", "Score": tpa})

            # TAM
            tam = TAMScorer.score(r["TAM"])
            for sub in ["PEOU", "PU", "Total"]:
                recs.append({"Condition": cond, "Scale": "TAM", "Subscale": sub, "Score": tam[sub]})

            # NASA-TLX
            tlx = NASATLXScorer.score(r["NASA_TLX"])
            for sub, v in tlx.items():
                recs.append({"Condition": cond, "Scale": "NASA-TLX", "Subscale": sub, "Score": v})

        return pd.DataFrame.from_records(recs)
