# --- describe.py ---
from __future__ import annotations
from typing import Callable, Dict, Iterable, Tuple
import numpy as np

def iqr(values: Iterable[float]) -> float:
    v = np.asarray(list(values), float)
    q75, q25 = np.percentile(v, [75, 25])
    return float(q75 - q25)

def bootstrap_ci(
    values: Iterable[float],
    stat_fn: Callable[[np.ndarray], float] = np.median,
    n_boot: int = 5000, alpha: float = 0.05, seed: int | None = None
) -> Tuple[float, float]:
    rng = np.random.default_rng(seed)
    v = np.asarray(list(values), float)
    boots = [stat_fn(rng.choice(v, size=len(v), replace=True)) for _ in range(n_boot)]
    lo, hi = np.percentile(boots, [100*alpha/2, 100*(1-alpha/2)])
    return float(lo), float(hi)

def describe(
    values: Iterable[float],
    with_ci: bool = False,
    stat_for_ci: str = "median",
    ci_seed: int | None = None
) -> Dict[str, float | int | tuple]:
    vals = list(values)
    n = len(vals)
    if n == 0:
        return {"mean": np.nan, "std": np.nan, "median": np.nan, "iqr": np.nan, "n": 0}
    mean = float(np.mean(vals))
    std  = float(np.std(vals, ddof=1)) if n > 1 else 0.0
    med  = float(np.median(vals))
    IQR  = iqr(vals)
    out: Dict[str, float | int | tuple] = {"mean": mean, "std": std, "median": med, "iqr": IQR, "n": n}
    if with_ci:
        stat_fn = np.median if stat_for_ci.lower() == "median" else np.mean
        out["ci95"] = bootstrap_ci(vals, stat_fn=stat_fn, seed=ci_seed)
    return out
