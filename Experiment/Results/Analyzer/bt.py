# --- bt.py ---
from __future__ import annotations
from typing import Dict, List, Sequence, Tuple
import numpy as np
import choix  # pip install choix

def _pairs_from_ranking(ranking: Sequence[str]) -> List[Tuple[str, str]]:
    pairs = []
    for i in range(len(ranking)):
        for j in range(i+1, len(ranking)):
            pairs.append((ranking[i], ranking[j]))  # 左 > 右
    return pairs

class BradleyTerry:
    """
    順位（例: ['P+SR','Pointing','SR','Label']）のリストから
    最尤推定＋ブートストラップCIを返すユーティリティ。
    """
    @staticmethod
    def fit_from_rankings(
        rankings: List[Sequence[str]],
        cond_order: List[str],
        n_boot: int = 1000,
        alpha: float = 0.05,
        seed: int | None = None
    ) -> Dict:
        rng = np.random.default_rng(seed)
        idx = {c: i for i, c in enumerate(cond_order)}
        edges = []
        for rnk in rankings:
            edges += [(idx[w], idx[l]) for (w, l) in _pairs_from_ranking(rnk)]

        theta = choix.ilsr_pairwise(len(cond_order), edges)
        theta = np.array(theta) - np.mean(theta)

        # 簡易対数尤度（相対比較用）
        def _loglik(th):
            s = 0.0
            for (w, l) in edges:
                s += np.log(1.0 / (1.0 + np.exp(th[l] - th[w])))
            return s

        ll = _loglik(theta)
        k_param = len(cond_order) - 1
        AIC = -2*ll + 2*k_param
        BIC = -2*ll + k_param * np.log(len(edges))

        # ブートストラップCI
        boots = []
        for _ in range(n_boot):
            samp = [edges[i] for i in rng.integers(0, len(edges), size=len(edges))]
            th_b = choix.ilsr_pairwise(len(cond_order), samp)
            th_b = np.array(th_b) - np.mean(th_b)
            boots.append(th_b)
        boots = np.vstack(boots)
        lo = np.percentile(boots, 2.5, axis=0)
        hi = np.percentile(boots, 97.5, axis=0)

        beta = {c: float(theta[idx[c]]) for c in cond_order}
        ci95 = {c: (float(lo[idx[c]]), float(hi[idx[c]])) for c in cond_order}

        ex = np.exp(theta - np.max(theta))
        probs = ex / ex.sum()
        scores = {c: float(probs[idx[c]]) for c in cond_order}

        return {"beta": beta, "ci95": ci95, "scores": scores, "AIC": float(AIC), "BIC": float(BIC)}
