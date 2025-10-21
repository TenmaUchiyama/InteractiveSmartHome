# --- analyzer.py ---
from __future__ import annotations
from typing import Dict, List
import pandas as pd

from dataset import SurveyDataset
from describe import describe
from scales import SUSScorer, TAMScorer, TPAScorer, NASATLXScorer
from stats import NonParametrics, cronbach_alpha
import numpy as np

class SurveyResultAnalyzer:
    """
    ファサード：既存の呼び出し口を維持しつつ、内部は分離。
    """
    def __init__(self, df: pd.DataFrame):
        self.ds = SurveyDataset(df)

    # ===== 記述統計 =====
    def describe_sus(self, with_ci: bool = False) -> Dict[str, Dict]:
        out = {}
        for cond in self.ds.condition_order:
            vals = [SUSScorer.score(r["SUS"]) for r in self.ds.rows if r["Condition"] == cond]
            out[cond] = describe(vals, with_ci=with_ci, stat_for_ci="median")
        return out

    def describe_tpa(self, with_ci: bool = False) -> Dict[str, Dict]:
        out = {}
        for cond in self.ds.condition_order:
            vals = [TPAScorer.score(r["TPA"]) for r in self.ds.rows if r["Condition"] == cond]
            out[cond] = describe(vals, with_ci=with_ci, stat_for_ci="median")
        return out

    def describe_tam(self, with_ci: bool = False) -> Dict[str, Dict[str, Dict]]:
        out = {}
        for cond in self.ds.condition_order:
            peou, pu, total = [], [], []
            for r in self.ds.rows:
                if r["Condition"] != cond:
                    continue
                s = TAMScorer.score(r["TAM"])
                peou.append(s["PEOU"]); pu.append(s["PU"]); total.append(s["Total"])
            out[cond] = {
                "PEOU":  describe(peou,  with_ci=with_ci, stat_for_ci="median"),
                "PU":    describe(pu,    with_ci=with_ci, stat_for_ci="median"),
                "Total": describe(total, with_ci=with_ci, stat_for_ci="median"),
            }
        return out

    def describe_nasa_tlx(self, with_ci: bool = False, scale_max: int = 21) -> Dict[str, Dict]:
        out: Dict[str, Dict] = {}
        for cond in self.ds.condition_order:
            dim_vals = {k: [] for k in ["Mental","Physical","Temporal","Performance","Effort","Frustration","Overall"]}
            for r in self.ds.rows:
                if r["Condition"] != cond:
                    continue
                tlx = NASATLXScorer.score(r["NASA_TLX"], scale_max=scale_max)
                for k, v in tlx.items():
                    dim_vals[k].append(v)
            out[cond] = {k: describe(vs, with_ci=with_ci, stat_for_ci="median") for k, vs in dim_vals.items()}
        return out

    # ===== ロング形式 =====
    def to_long_scores(self) -> pd.DataFrame:
        return self.ds.to_long_scores()

    # ===== 推測統計 =====
    def friedman_nemenyi(self, scale: str = "SUS", subscale: str = "Total") -> Dict:
        long_df = self.to_long_scores()
        return NonParametrics.friedman_nemenyi(long_df, scale, subscale, self.ds.condition_order)

    def nemenyi_table(self, scale: str = "SUS", subscale: str = "Total") -> pd.DataFrame:
        res = self.friedman_nemenyi(scale, subscale)
        return res["nemenyi"].copy()

    def significant_pairs(
        self, scale: str = "SUS", subscale: str = "Total",
        alpha_low: float = 0.01, alpha_high: float = 0.05, unique: bool = True
    ) -> Dict[str, List[tuple]]:
        res = self.friedman_nemenyi(scale, subscale)
        return NonParametrics.significant_pairs(res["nemenyi"], self.ds.condition_order, alpha_low, alpha_high, unique)

    # ===== 信頼性（尺度横断で） =====
    def reliability_all(self) -> Dict[str, float]:
        import numpy as np
        sus_items, tam_items, tpa_items, tlx_items = [], [], [], []
        for r in self.ds.rows:
            sus_items.append(np.asarray(r["SUS"], float))
            tam_items.append(np.asarray(r["TAM"], float))
            tpa_items.append(np.asarray(r["TPA"], float))
            tlx_items.append(np.asarray(r["NASA_TLX"], float))
        return {
            "SUS":      cronbach_alpha(np.vstack(sus_items)),
            "TAM":      cronbach_alpha(np.vstack(tam_items)),
            "TPA":      cronbach_alpha(np.vstack(tpa_items)),
            "NASA_TLX": cronbach_alpha(np.vstack(tlx_items)),
        }
