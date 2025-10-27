# dataset.py
from __future__ import annotations
from dataclasses import dataclass
from typing import List, Dict, Optional
import numpy as np
import pandas as pd

@dataclass
class SurveySchema:
    # ← あなたのCSVの列位置に合わせて必要なら調整
    sus_cols:       List[int] = list(range(3, 13))     # SUS 10項目 (1-5)
    tam_cols:       List[int] = list(range(13, 25))    # TAM 12項目 (1-7) PEOU(前6) / PU(後6)
    tpa_cols:       List[int] = list(range(25, 37))    # TPA 12項目 (1-7) 前7=ポジ / 後5=ネガ反転
    nasatlx_cols:   List[int] = list(range(37, 43))    # NASA-TLX 6項目 (1-21) Performance反転
    condition_col:  int | str = 2                      # Condition 列
    subject_col:    Optional[int | str] = "ID"         # 被験者ID（無ければ自動採番）

class SurveyDataset:
    # 正規化（数値/英語/日本語）
    _ID2NAME = {1:"SR", 2:"P+SR", 3:"Pointing", 4:"Label"}
    _ALIASES = {
        "SR":"SR","SpatialReference":"SR","空間参照だけ":"SR",
        "P+SR":"P+SR","SpatialReference+Pointing":"P+SR","Pointing + 空間参照":"P+SR",
        "Pointing":"Pointing",
        "Label":"Label","ラベル":"Label",
    }

    def __init__(self, df: pd.DataFrame, schema: SurveySchema = SurveySchema()):
        self.df = df.reset_index(drop=True).copy()
        self.schema = schema
        self._prepare_subject()
        self.rows = self._rows_from_df()
        # 出力順（図表と統一）
        self.condition_order = ["SR", "P+SR", "Pointing", "Label"]

    # ---- public ----
    def to_long_scores(self) -> pd.DataFrame:
        recs = []
        for r in self.rows:
            c = r["Condition"]
            s = r["Subject"]
            # SUS
            recs.append({"Subject":s,"Condition":c,"Scale":"SUS","Subscale":"Total","Score":_sus_score(r["SUS"])})
            # TPA
            recs.append({"Subject":s,"Condition":c,"Scale":"TPA","Subscale":"Total","Score":_tpa_score(r["TPA"])})
            # TAM
            tam = _tam_score(r["TAM"])
            for sub in ("PEOU","PU","Total"):
                recs.append({"Subject":s,"Condition":c,"Scale":"TAM","Subscale":sub,"Score":tam[sub]})
            # NASA-TLX
            tlx = _nasatlx_score(r["NASA_TLX"])
            for k in ["Mental","Physical","Temporal","Performance","Effort","Frustration"]:
                recs.append({"Subject":s,"Condition":c,"Scale":"NASA-TLX","Subscale":k,"Score":tlx[k]})
            recs.append({"Subject":s,"Condition":c,"Scale":"NASA-TLX","Subscale":"Overall","Score":tlx["Overall"]})
        return pd.DataFrame.from_records(recs)

    # ---- internal ----
    def _prepare_subject(self):
        if (self.schema.subject_col in self.df.columns):
            col = self.df[self.schema.subject_col]
            self.df["_SUBJECT_"] = col.astype(str).where(~col.isna(), None)
        else:
            # 4行=1人 前提のフォールバック
            self.df["_SUBJECT_"] = [f"S{idx//4:03d}" for idx in range(len(self.df))]

    def _norm_condition(self, v) -> Optional[str]:
        if pd.isna(v): return None
        # 数値ID
        try:
            return self._ID2NAME.get(int(v))
        except Exception:
            pass
        # 文字列
        s = str(v).strip()
        return self._ALIASES.get(s)

    def _slice(self, row: pd.Series, cols: List[int]) -> List[float]:
        vals = []
        for c in cols:
            try:
                vals.append(float(row.iloc[c]))
            except Exception:
                vals.append(np.nan)
        return vals

    def _rows_from_df(self) -> List[Dict]:
        out = []
        for i in range(len(self.df)):
            row = self.df.iloc[i]
            cond = self._norm_condition(row[self.schema.condition_col])
            if cond is None:  # Conditionが読めない行は捨てる
                continue
            out.append({
                "Subject":  row["_SUBJECT_"],
                "Condition":cond,
                "SUS":      self._slice(row, self.schema.sus_cols),
                "TAM":      self._slice(row, self.schema.tam_cols),
                "TPA":      self._slice(row, self.schema.tpa_cols),
                "NASA_TLX": self._slice(row, self.schema.nasatlx_cols),
            })
        return out


# ==== スコア計算（datasetからも使う） ====
def _sus_score(items: List[float]) -> float:
    # 10項目: 奇数(1,3,5,7,9) r-1、偶数 5-r、合計×2.5
    s = []
    for i, r in enumerate(items, start=1):
        if not np.isfinite(r): continue
        s.append((r - 1) if (i % 2 == 1) else (5 - r))
    return float(np.sum(s) * 2.5) if s else np.nan

def _tam_score(items: List[float]) -> Dict[str,float]:
    arr = np.asarray(items, float)
    if arr.size < 12:
        return {"PEOU":np.nan,"PU":np.nan,"Total":np.nan}
    return {
        "PEOU": float(np.nanmean(arr[:6])),
        "PU":   float(np.nanmean(arr[6:12])),
        "Total":float(np.nanmean(arr[:12])),
    }

def _tpa_score(items: List[float]) -> float:
    arr = np.asarray(items, float)
    if arr.size < 12: return np.nan
    pos7 = arr[:7]
    neg5 = 8.0 - arr[7:12]  # 7件法の反転
    allv = np.concatenate([pos7, neg5])
    return float(np.nanmean(allv))

def _nasatlx_score(items: List[float], scale_max=21) -> Dict[str,float]:
    arr = np.asarray(items, float)
    if arr.size < 6:
        return {k:np.nan for k in ["Mental","Physical","Temporal","Performance","Effort","Frustration","Overall"]}
    perf_rev = (scale_max + 1) - arr[3]
    dims = {
        "Mental":       float(arr[0]),
        "Physical":     float(arr[1]),
        "Temporal":     float(arr[2]),
        "Performance":  float(perf_rev),
        "Effort":       float(arr[4]),
        "Frustration":  float(arr[5]),
    }
    dims["Overall"] = float(np.nanmean(list(dims.values())))
    return dims
