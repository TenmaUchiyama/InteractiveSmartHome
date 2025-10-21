# --- stats.py ---
from __future__ import annotations
from typing import Dict, Iterable, List
import numpy as np
import pandas as pd
from scipy.stats import friedmanchisquare
import scikit_posthocs as sp

def kendalls_w_from_friedman(chi2: float, n_blocks: int, k: int) -> float:
    # W = chi2 / (n * (k - 1))
    return float(chi2 / (n_blocks * (k - 1)))

def cronbach_alpha(items_2d: np.ndarray) -> float:
    """
    items_2d: shape=(n_subjects, n_items)
    """
    X = np.asarray(items_2d, float)
    k = X.shape[1]
    var_items = X.var(axis=0, ddof=1).sum()
    var_total = X.sum(axis=1).var(ddof=1)
    if var_total == 0 or k <= 1:
        return float("nan")
    return float(k/(k-1) * (1 - var_items/var_total))

class NonParametrics:
    """Friedman + Nemenyi をロング形式データから算出"""

    @staticmethod
    def friedman_nemenyi(
        long_df: pd.DataFrame, scale: str, subscale: str, cond_order: List[str]
    ) -> Dict:
        df = long_df[(long_df["Scale"] == scale) & (long_df["Subscale"] == subscale)].reset_index(drop=True)
        if df.empty:
            raise ValueError(f"No data for scale={scale}, subscale={subscale}.")
        if len(df) % len(cond_order) != 0:
            raise ValueError(f"Row count ({len(df)}) is not a multiple of {len(cond_order)}.")
        df["Subject"] = df.index // len(cond_order)

        wide = df.pivot(index="Subject", columns="Condition", values="Score").reindex(columns=cond_order).dropna()
        if wide.empty or wide.shape[1] != len(cond_order):
            raise ValueError("Not enough complete blocks.")

        arrays = [wide[c].values for c in cond_order]
        chi2, p = friedmanchisquare(*arrays)
        n = wide.shape[0]
        k = len(cond_order)
        W = kendalls_w_from_friedman(chi2, n, k)

        nemenyi = sp.posthoc_nemenyi_friedman(wide[cond_order])
        # 整形（列/行順を固定）
        nemenyi = nemenyi.loc[cond_order, cond_order]
        return {"friedman": {"chi2": float(chi2), "df": k-1, "p": float(p), "W": W, "n": n, "k": k},
                "nemenyi": nemenyi}

    @staticmethod
    def significant_pairs(
        nemenyi_p: pd.DataFrame,
        cond_order: List[str],
        alpha_low: float = 0.01,
        alpha_high: float = 0.05,
        unique: bool = True
    ) -> Dict[str, List[tuple]]:
        pmat = nemenyi_p.loc[cond_order, cond_order]
        n = len(cond_order)
        if unique:
            idx_pairs = [(i, j) for i in range(n) for j in range(i+1, n)]
        else:
            idx_pairs = [(i, j) for i in range(n) for j in range(n) if i != j]
        out = {"0.01": [], "0.05": []}
        for i, j in idx_pairs:
            p = float(pmat.iat[i, j])
            a, b = cond_order[i], cond_order[j]
            if p <= alpha_low:
                out["0.01"].append((a, b))
            elif p <= alpha_high:
                out["0.05"].append((a, b))
        return out
