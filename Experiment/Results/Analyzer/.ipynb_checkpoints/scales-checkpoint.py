# --- scales.py ---
from __future__ import annotations
from typing import Dict, List
import numpy as np

class SUSScorer:
    """SUS: 10項目, 奇数(r-1) 偶数(5-r), 合計×2.5 → 0-100"""
    @staticmethod
    def score(responses: List[float]) -> float:
        scores = [(r - 1 if (i % 2 == 1) else 5 - r) for i, r in enumerate(responses, start=1)]
        return float(sum(scores) * 2.5)

class TAMScorer:
    """TAM: 12項目, 前6=PEOU, 後6=PU, Total=全体平均"""
    @staticmethod
    def score(responses: List[float]) -> Dict[str, float]:
        arr = np.asarray(responses, float)
        peou = float(arr[:6].mean())
        pu   = float(arr[6:].mean())
        total = float(arr.mean())
        return {"PEOU": peou, "PU": pu, "Total": total}

class TPAScorer:
    """
    TPA: 12項目, [pos(7項目), neg(5項目)], 後半5を反転(1..7 → 8-r)
    返り値は Total のみ（必要なら拡張可）
    """
    @staticmethod
    def score(responses: List[float]) -> float:
        pos = responses[:7]
        neg = [8 - r for r in responses[7:]]  # 5項目反転
        return float(np.mean(pos + neg))

class NASATLXScorer:
    """
    NASA-TLX: [Mental, Physical, Temporal, Performance(反転), Effort, Frustration]
    既定スケール1..21, Performanceのみ scale_max+1-r で反転
    """
    @staticmethod
    def score(responses: List[float], scale_max: int = 21) -> Dict[str, float]:
        dims = {
            "Mental":       float(responses[0]),
            "Physical":     float(responses[1]),
            "Temporal":     float(responses[2]),
            "Performance":  float((scale_max + 1) - responses[3]),  # 反転済み
            "Effort":       float(responses[4]),
            "Frustration":  float(responses[5]),
        }
        overall = float(np.mean(list(dims.values())))
        return {**dims, "Overall": overall}
