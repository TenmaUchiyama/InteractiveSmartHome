#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
IMWUT-grade analysis (clean, single-file):
- Descriptives & plots (matplotlib only; no seaborn)
- Friedman + Wilcoxon (Holm)
- MixedLM for log(T_task_sec) with condition/devices/colors (+ interaction)
- GEE for N_cmds (family auto: Poisson or NegBin by overdispersion)
- Model-based pairwise contrasts for condition (Holm)
- Mediation hint (N_cmds as mediator for logT)
- Robustness check (top 1% trim)
- Within-subject Spearman (centering)
- Markdown report to ./imwut_results/analysis_report.md

Inputs (must exist):
  ./imwut_results/task_summary_enriched.csv
  (optionally) ./imwut_results/participant_condition_summary.csv

Assumed columns in task-level CSV:
  participant (or participant_id / subject / user_id),
  condition, taskId, T_task_sec, N_cmds, N_actuations,
  n_devices_final (proxy) and/or n_devices_required,
  n_colors_required

Additionally merges:
  ./RESULTS/P{participant}_{1..4}/*.json  (attempt array per file)
  -> sums metrics.tokens.system_total_time_sec per (participant, condition, taskId)
"""

import os
import re
import glob
import json
import math
import warnings
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt

from scipy.stats import friedmanchisquare, wilcoxon, spearmanr, norm

import statsmodels.api as sm
import statsmodels.formula.api as smf
from statsmodels.genmod.generalized_estimating_equations import GEE
from statsmodels.genmod.families import Poisson, NegativeBinomial
from statsmodels.genmod.cov_struct import Exchangeable

warnings.filterwarnings("ignore")

# ----------------- Constants & IO -----------------
OUT_DIR = "./imwut_results"
PLOT_DIR = os.path.join(OUT_DIR, "plots")
os.makedirs(PLOT_DIR, exist_ok=True)

# 条件の順序とマップ
COND_ORDER = ["SR", "P+SR", "Pointing", "Label"]
COND_MAP   = {1: "SR", 2: "P+SR", 3: "Pointing", 4: "Label"}

TASKS_CSV = os.path.join(OUT_DIR, "task_summary_enriched.csv")
PC_CSV    = os.path.join(OUT_DIR, "participant_condition_summary.csv")
REPORT_MD = os.path.join(OUT_DIR, "analysis_report.md")

RESULTS_ROOT = "./RESULTS"

# ----------------- Small utils -----------------
def is_json_file(path: str) -> bool:
    b = os.path.basename(path)
    return b.endswith(".json") and not b.endswith(".json.meta")

def safe_float(x, default=None):
    if x is None:
        return default
    try:
        if isinstance(x, (int, float)):
            return float(x)
        s = str(x).strip()
        if s == "" or s.lower() == "nan":
            return default
        return float(s)
    except Exception:
        return default

def safe_get(d, *keys, default=None):
    cur = d
    try:
        for k in keys:
            if cur is None:
                return default
            cur = cur.get(k)
        return cur if cur is not None else default
    except Exception:
        return default

# ----------------- Helpers -----------------
def ensure_participant_column(df: pd.DataFrame) -> pd.DataFrame:
    """Find a participant-like column and normalize to 'participant'."""
    if "participant" in df.columns:
        return df
    for alt in ["participant_id", "subject", "user_id", "id"]:
        if alt in df.columns:
            return df.rename(columns={alt: "participant"})
    raise KeyError("No participant column found. Expected one of: participant, participant_id, subject, user_id, id")

def ecdf(arr):
    x = np.array([v for v in arr if pd.notna(v)])
    x.sort()
    if len(x) == 0:
        return np.array([]), np.array([])
    y = np.arange(1, len(x)+1) / len(x)
    return x, y

def holm_adjust(pvals):
    pvals = np.asarray(pvals, dtype=float)
    m = len(pvals)
    order = np.argsort(pvals)
    out = np.zeros_like(pvals)
    prev = 0.0
    for k, idx in enumerate(order):
        adj = (m - k) * pvals[idx]
        adj = max(adj, prev)
        out[idx] = min(adj, 1.0)
        prev = out[idx]
    return out

def effect_r_from_W(W, n):
    mean_W = n * (n + 1) / 4.0
    var_W  = n * (n + 1) * (2 * n + 1) / 24.0
    if var_W <= 0:
        return 0.0
    z = (W - mean_W) / math.sqrt(var_W)
    return abs(z) / math.sqrt(n)

def winsorize_top(x, top=0.01):
    if len(x) == 0: return x
    q = np.quantile(x, 1-top)
    return np.clip(x, None, q)

def within_center(df: pd.DataFrame, value_col: str, by_col: str):
    """対象列をby_col単位でセンタリング"""
    return df[value_col] - df.groupby(by_col)[value_col].transform("mean")

def boxstrip(ax, values_by_group, labels, title, ylabel):
    ax.boxplot(values_by_group, labels=labels, showfliers=False)
    # strip (no color specification)
    for i, vals in enumerate(values_by_group, start=1):
        if vals is None or len(vals)==0: continue
        xj = np.random.normal(loc=i, scale=0.08, size=len(vals))
        ax.plot(xj, vals, 'o', alpha=0.6, markersize=3)
    ax.set_title(title)
    ax.set_ylabel(ylabel)
    ax.grid(alpha=0.3)

# ----------------- RESULTS aggregator -----------------
def load_server_attempt_times_from_results(results_root: str):
    """
    ./RESULTS/P20_1 のようなディレクトリ構成を走査し、
    各 JSON(=attempt配列)から metrics.tokens.system_total_time_sec を合算。
    戻り値: list[dict] (participant, condition, taskId, system_time_total_sec, n_attempts_server)
    """
    out_rows = []

    if not os.path.isdir(results_root):
        print(f"[WARN] RESULTS dir not found: {results_root}")
        return out_rows

    # ディレクトリ名: P{num}_{condNum}  (例: P20_1)
    dirnames = [d for d in sorted(os.listdir(results_root)) if os.path.isdir(os.path.join(results_root, d))]
    pat = re.compile(r"^P(\d+)_([1-4])$")

    # key=(participant, condition_label, taskId) -> {sum, n}
    agg = {}

    for dn in dirnames:
        m = pat.match(dn)
        if not m:
            continue
        participant_id = int(m.group(1))
        cond_num = int(m.group(2))
        if cond_num not in COND_MAP:
            continue
        condition = COND_MAP[cond_num]

        dpath = os.path.join(results_root, dn)
        json_files = [fp for fp in glob.glob(os.path.join(dpath, "*.json")) if is_json_file(fp)]
        if not json_files:
            # 一段下のサブディレクトリも軽く探す
            subdirs = [sd for sd in sorted(os.listdir(dpath)) if os.path.isdir(os.path.join(dpath, sd))]
            for sd in subdirs:
                json_files.extend([fp for fp in glob.glob(os.path.join(dpath, sd, "*.json")) if is_json_file(fp)])

        for fp in json_files:
            try:
                with open(fp, "r", encoding="utf-8") as f:
                    data = json.load(f)
            except Exception as e:
                print(f"[WARN] failed to read {fp}: {e}")
                continue

            # 期待: トップレベルが配列（attemptのリスト）
            # 念のため { "results": [...] } にも耐性
            if isinstance(data, dict) and "results" in data and isinstance(data["results"], list):
                items = data["results"]
            elif isinstance(data, list):
                items = data
            else:
                continue

            for item in items:
                if not isinstance(item, dict):
                    continue

                task_id = item.get("task_id") or item.get("taskId")
                if not task_id:
                    continue

                # 最優先: metrics.tokens.system_total_time_sec
                sys_t = safe_get(item, "metrics", "tokens", "system_total_time_sec", default=None)
                if sys_t is None:
                    sys_t = safe_get(item, "metrics", "system_total_time_sec", default=None)

                sys_t = safe_float(sys_t, default=None)
                if sys_t is None:
                    continue

                key = (participant_id, condition, task_id)
                if key not in agg:
                    agg[key] = {"sum": 0.0, "n": 0}
                agg[key]["sum"] += sys_t
                agg[key]["n"] += 1

    for (pid, cond, tid), v in agg.items():
        out_rows.append({
            "participant": pid,
            "condition": cond,
            "taskId": tid,
            "system_time_total_sec": v["sum"],   # タスク内の全attempt合計
            "n_attempts_server": v["n"],        # 合算attempt数
        })
    return out_rows

# ----------------- Load & merge -----------------
def load_inputs():
    if not os.path.exists(TASKS_CSV):
        raise FileNotFoundError(f"Missing: {TASKS_CSV}")

    # 1) タスクCSV読み込み
    df_tasks = pd.read_csv(TASKS_CSV)
    df_tasks = ensure_participant_column(df_tasks)

    # devices proxy normalize
    if "n_devices_required" not in df_tasks.columns and "n_devices_final" in df_tasks.columns:
        df_tasks["n_devices_required"] = df_tasks["n_devices_final"]

    # 2) RESULTS から system_total_time_sec を集計してマージ
    server_rows = load_server_attempt_times_from_results(RESULTS_ROOT)
    if server_rows:
        df_server = pd.DataFrame(server_rows)  # (participant, condition, taskId, system_time_total_sec, n_attempts_server)
        df_tasks = df_tasks.merge(df_server, on=["participant","condition","taskId"], how="left")
    else:
        print("[WARN] No server attempt time rows merged.")

    # 3) 参加者×条件メディアンは df_tasks から再計算（新指標も含めて）
    #    PC_CSV は後方互換で読み取るが、最終的には df_tasks から作り直す
    if os.path.exists(PC_CSV):
        try:
            _ = pd.read_csv(PC_CSV)  # 参照のみ
        except Exception:
            pass

    # まとめ対象列（存在するもののみを使う）
    agg_cols = [c for c in [
        "T_task_sec","N_cmds","N_actuations",
        "n_devices_final","n_devices_required","n_colors_required",
        "system_time_total_sec","n_attempts_server"
    ] if c in df_tasks.columns]

    df_pc = (df_tasks
             .groupby(["participant","condition"], as_index=False)[agg_cols]
             .median(numeric_only=True))

    return df_tasks, df_pc

# ----------------- Descriptive Plots -----------------
def plot_descriptive(df_pc: pd.DataFrame):
    metrics = [m for m in ["T_task_sec", "N_cmds", "N_actuations", "system_time_total_sec"] if m in df_pc.columns]
    for metric in metrics:
        vals = [df_pc[df_pc["condition"]==c][metric].dropna().values for c in COND_ORDER]
        fig, ax = plt.subplots(figsize=(6.5,4.2))
        boxstrip(ax, vals, COND_ORDER, f"{metric} by Condition", metric)
        fig.tight_layout()
        fig.savefig(os.path.join(PLOT_DIR, f"boxstrip_{metric}.png"), dpi=220)
        plt.close(fig)

        # ECDF
        fig, ax = plt.subplots(figsize=(6.5,4.2))
        for c in COND_ORDER:
            arr = df_pc[df_pc["condition"]==c][metric].dropna().values
            x,y = ecdf(arr)
            if len(x)==0: continue
            ax.plot(x,y,label=c)
        ax.set_title(f"ECDF of {metric} by Condition")
        ax.set_xlabel(metric); ax.set_ylabel("F(x)")
        ax.legend(); ax.grid(alpha=0.3)
        fig.tight_layout()
        fig.savefig(os.path.join(PLOT_DIR, f"ecdf_{metric}.png"), dpi=220)
        plt.close(fig)

# ----------------- Friedman + Wilcoxon -----------------
def friedman_block(df_pc: pd.DataFrame, metric: str):
    if metric not in df_pc.columns:
        return None, None, None, None
    pivot = df_pc.pivot(index="participant", columns="condition", values=metric)
    pivot = pivot.reindex(columns=COND_ORDER).dropna(how="any")
    if len(pivot)==0:
        return None, None, None, None
    arrays = [pivot[c].values for c in COND_ORDER]
    stat, p = friedmanchisquare(*arrays)

    # Pairwise Wilcoxon + Holm
    from itertools import combinations
    pairs, Ws, pvals, ns, rs = [], [], [], [], []
    for a,b in combinations(COND_ORDER, 2):
        x = pivot[a].values; y = pivot[b].values
        n = len(x)
        try:
            W, pw = wilcoxon(x, y, zero_method='wilcox', correction=False)
        except Exception:
            W, pw = (0.0, 1.0)
        r = effect_r_from_W(W, n)
        pairs.append(f"{a} vs {b}")
        Ws.append(W)
        pvals.append(pw)
        ns.append(n)
        rs.append(r)
    p_holm = holm_adjust(pvals).tolist()
    table = pd.DataFrame({"pair":pairs, "n":ns, "W":Ws, "p_raw":pvals, "p_holm":p_holm, "effect_r":rs})
    return stat, p, pivot, table

# ----------------- Modeling -----------------
def prepare_model_frame(df_tasks: pd.DataFrame) -> pd.DataFrame:
    dfm = df_tasks.copy()
    required = ["participant","condition","T_task_sec","N_cmds","n_devices_required"]
    missing = [c for c in required if c not in dfm.columns]
    if missing:
        raise KeyError(f"Missing columns in task CSV: {missing}")

    dfm["logT"] = np.log(dfm["T_task_sec"].clip(lower=1e-6))

    # center predictors for stability
    for col in ["n_devices_required","n_colors_required"]:
        if col not in dfm.columns:
            dfm[col] = np.nan
        mu = dfm[col].dropna().mean() if dfm[col].notna().any() else 0.0
        dfm[col+"_c"] = dfm[col] - mu
    return dfm

def fit_mixedlm_logT(dfm: pd.DataFrame):
    sub = dfm.dropna(subset=["logT","n_devices_required_c","n_colors_required_c"])
    if sub.empty:
        return None
    try:
        formula = "logT ~ C(condition) + n_devices_required_c + n_colors_required_c + n_devices_required_c:C(condition)"
        m = smf.mixedlm(formula, data=sub, groups=sub["participant"])
        return m.fit(method="lbfgs", reml=False)
    except Exception as e:
        print("[WARN] MixedLM(logT) failed:", e)
        return None

def summarize_mixedlm_logT(res) -> str:
    if res is None:
        return "MixedLM(logT) did not converge or no data.\n"
    lines = []
    lines.append("### MixedLM: log(T_task_sec) ~ condition + devices + colors + condition×devices\n")
    lines.append("Fixed effects (β), 95%CI, and % change (exp(β)-1)*100.\n")
    params = res.params
    conf = res.conf_int()
    rows = []
    for name, beta in params.items():
        ci = conf.loc[name].values if name in conf.index else [np.nan, np.nan]
        pct = (math.exp(beta)-1)*100
        rows.append([name, beta, ci[0], ci[1], pct])
    out = pd.DataFrame(rows, columns=["term","beta","ci_low","ci_high","pct_change"])
    lines.append(out.to_string(index=False))
    lines.append("\n")
    return "\n".join(lines)

def choose_family_by_overdispersion(dfm: pd.DataFrame):
    y = dfm["N_cmds"].dropna().values
    if len(y)==0: return Poisson()
    mu = np.mean(y)
    va = np.var(y, ddof=1) if len(y)>1 else mu
    return NegativeBinomial() if va > mu*1.2 else Poisson()

def fit_gee_ncmds(dfm: pd.DataFrame):
    sub = dfm.dropna(subset=["N_cmds","n_devices_required_c","n_colors_required_c"])
    if sub.empty:
        return None
    try:
        family = choose_family_by_overdispersion(sub)
        formula = "N_cmds ~ C(condition) + n_devices_required_c + n_colors_required_c + n_devices_required_c:C(condition)"
        model = GEE.from_formula(formula, groups="participant", data=sub, cov_struct=Exchangeable(), family=family)
        return model.fit()
    except Exception as e:
        print("[WARN] GEE failed:", e)
        return None

def summarize_gee(res) -> str:
    if res is None:
        return "GEE(N_cmds) failed or no data.\n"
    lines = []
    lines.append(f"### GEE: N_cmds ~ condition + devices + colors + condition×devices  (family={res.model.family.__class__.__name__})\n")
    lines.append("Coefficients as IRR=exp(β), 95%CI.\n")
    params = res.params
    conf = res.conf_int()
    rows = []
    for name, beta in params.items():
        ci = conf.loc[name].values if name in conf.index else [np.nan, np.nan]
        irr, lo, hi = math.exp(beta), math.exp(ci[0]), math.exp(ci[1])
        rows.append([name, beta, ci[0], ci[1], irr, lo, hi])
    out = pd.DataFrame(rows, columns=["term","beta","ci_low","ci_high","IRR","IRR_low","IRR_high"])
    lines.append(out.to_string(index=False))
    lines.append("\n")
    return "\n".join(lines)

def pairwise_condition_contrasts_gee(res):
    """Model-based pairwise contrasts for condition at centered covariates=0."""
    if res is None:
        return None
    names = res.model.exog_names

    def coef_for_cond(level):
        v = np.zeros(len(names))
        if "Intercept" in names:
            v[names.index("Intercept")] = 1.0
        for n in names:
            # e.g., C(condition)[T.P+SR]
            if n.startswith("C(condition)") and f"[T.{level}]" in n:
                v[names.index(n)] = 1.0
        return v

    rows = []
    from itertools import combinations
    for a, b in combinations(COND_ORDER, 2):
        va = coef_for_cond(a); vb = coef_for_cond(b)
        diff = va - vb
        est = float(diff @ res.params.values)
        V = res.cov_params()
        se = math.sqrt(float(diff.T @ V.values @ diff))
        z  = (est / se) if se > 0 else 0.0
        p  = 2 * (1 - norm.cdf(abs(z)))
        irr_ratio = math.exp(est)  # ratio of IRRs between conditions a and b
        rows.append([f"{a} vs {b}", est, se, z, p, irr_ratio])
    dfc = pd.DataFrame(rows, columns=["pair","est","se","z","p_raw","IRR_ratio"])
    dfc["p_holm"] = holm_adjust(dfc["p_raw"].values)
    return dfc

def mediation_hint(dfm: pd.DataFrame) -> str:
    sub = dfm[["participant","condition","logT","N_cmds","n_devices_required_c","n_colors_required_c"]].dropna()
    if sub.empty:
        return "Mediation: insufficient data.\n"
    try:
        m_red  = smf.mixedlm("logT ~ C(condition) + n_devices_required_c + n_colors_required_c", sub, groups=sub["participant"]).fit(method="lbfgs", reml=False)
        m_full = smf.mixedlm("logT ~ C(condition) + n_devices_required_c + n_colors_required_c + N_cmds", sub, groups=sub["participant"]).fit(method="lbfgs", reml=False)
        def coef_table(res, tag):
            coefs = res.params.filter(like="C(condition)")
            return pd.DataFrame({"term":coefs.index, tag:coefs.values})
        t = coef_table(m_red,"beta_reduced").merge(coef_table(m_full,"beta_full"), on="term", how="outer")
        t["shrink"] = (t["beta_reduced"].abs() - t["beta_full"].abs()) / t["beta_reduced"].abs()
        return "### Mediation (heuristic): effect shrinkage when adding N_cmds\n" + t.to_string(index=False) + "\n"
    except Exception as e:
        return f"Mediation failed: {e}\n"

def robustness_trim_logT(dfm: pd.DataFrame):
    sub = dfm.copy()
    sub["T_trim"] = winsorize_top(sub["T_task_sec"].values, top=0.01)
    sub["logT_trim"] = np.log(sub["T_trim"].clip(lower=1e-6))
    sub = sub.dropna(subset=["logT_trim","n_devices_required_c","n_colors_required_c"])
    if sub.empty:
        return None
    try:
        res = smf.mixedlm("logT_trim ~ C(condition) + n_devices_required_c + n_colors_required_c + n_devices_required_c:C(condition)",
                          sub, groups=sub["participant"]).fit(method="lbfgs", reml=False)
        return res
    except Exception:
        return None

def within_subject_spearman(df_tasks: pd.DataFrame) -> str:
    """被験者内センタリング後のスピアマン相関（IMWUT論文用解析）"""
    sub = ensure_participant_column(df_tasks.copy())

    sub["logT"] = np.log(sub["T_task_sec"].clip(lower=1e-6))
    if "n_devices_required" not in sub.columns and "n_devices_final" in sub.columns:
        sub["n_devices_required"] = sub["n_devices_final"]

    # センタリング
    sub["logT_w"] = within_center(sub, "logT", "participant")
    sub["dev_w"]  = within_center(sub, "n_devices_required", "participant")
    if "n_colors_required" in sub.columns:
        sub["col_w"] = within_center(sub, "n_colors_required", "participant")
    else:
        sub["col_w"] = np.nan

    # 相関
    out = []
    for y, x in [("logT_w", "dev_w"), ("logT_w", "col_w")]:
        s = sub[[y, x]].dropna()
        if len(s) >= 5:
            rho, p = spearmanr(s[y], s[x])
            out.append(f"- within Spearman ρ({y}, {x}) = {rho:.3f}, p={p:.3g}, N={len(s)}")
        else:
            out.append(f"- within Spearman ρ({y}, {x}) = n/a (N<{len(s)})")
    return "\n".join(out) + "\n"

# ----------------- Report blocks -----------------
def write_friedman_blocks(lines, df_pc: pd.DataFrame):
    # 分析対象に system_time_total_sec がある場合は含める
    metrics = [m for m in ["T_task_sec","N_cmds","N_actuations","system_time_total_sec"] if m in df_pc.columns]
    for metric in metrics:
        stat, p, pivot, pairs = friedman_block(df_pc, metric)
        lines.append(f"\n## {metric}\n")
        if stat is None:
            lines.append("有効データ不足\n"); continue
        lines.append(f"\nFriedman: χ²={stat:.4f}, p={p:.6g}, N={len(pivot)}\n")
        # 記述統計
        lines.append("\n### 記述統計（参加者×条件メディアン）\n")
        lines.append("Condition | Median | Q1 | Q3 | Mean | SD")
        lines.append("---|---:|---:|---:|---:|---:")
        for c in COND_ORDER:
            vals = pivot[c].dropna().values if c in pivot.columns else np.array([])
            if len(vals)==0:
                lines.append(f"{c} |  |  |  |  | "); continue
            med = np.median(vals); q1=np.percentile(vals,25); q3=np.percentile(vals,75)
            mu = np.mean(vals); sd = np.std(vals, ddof=1) if len(vals)>1 else 0.0
            lines.append(f"{c} | {med:.3g} | {q1:.3g} | {q3:.3g} | {mu:.3g} | {sd:.3g}")
        # ペア
        if p < 0.05 and pairs is not None:
            lines.append("\n### ペアごとの Wilcoxon（Holm 補正）\n")
            lines.append("Pair | n | W | p_raw | p_holm | effect_r")
            lines.append("---|---:|---:|---:|---:|---:")
            for _, r in pairs.iterrows():
                lines.append(f"{r['pair']} | {int(r['n'])} | {r['W']:.3g} | {r['p_raw']:.3g} | {r['p_holm']:.3g} | {r['effect_r']:.3g}")
        else:
            lines.append("\n（Friedman で有意差なし → ペア検定省略）\n")

def write_models_blocks(lines, dfm: pd.DataFrame):
    """
    統計モデル（MixedLM, GEE, 媒介, ロバスト性, 色×条件交互作用）をまとめて出力。
    """
    # MixedLM: log(T_task_sec)
    mixed_logT = fit_mixedlm_logT(dfm)
    lines.append("\n## 混合効果モデル：log(T_task_sec)\n")
    lines.append(summarize_mixedlm_logT(mixed_logT))

    # GEE: N_cmds
    gee_res = fit_gee_ncmds(dfm)
    lines.append("\n## GEE：N_cmds（カウント；過分散に応じ Poisson/NB）\n")
    lines.append(summarize_gee(gee_res))

    # GEE ペア比較
    gee_pairs = pairwise_condition_contrasts_gee(gee_res)
    if gee_pairs is not None and len(gee_pairs):
        lines.append("\n### 条件のモデルベース比較（IRR比；Holm補正）\n")
        lines.append("Pair | est | se | z | p_raw | p_holm | IRR_ratio")
        lines.append("---|---:|---:|---:|---:|---:|---:")
        for _, r in gee_pairs.iterrows():
            lines.append(
                f"{r['pair']} | {r['est']:.3g} | {r['se']:.3g} | {r['z']:.3g} | {r['p_raw']:.3g} | {r['p_holm']:.3g} | {r['IRR_ratio']:.3g}"
            )

    # 媒介
    lines.append("\n## 媒介の示唆（N_cmds が logT を媒介するか）\n")
    lines.append(mediation_hint(dfm))

    # ロバスト性
    lines.append("\n## ロバスト性（上位1%トリムの再推定）\n")
    rob = robustness_trim_logT(dfm)
    lines.append(summarize_mixedlm_logT(rob))

    # 色×条件 交互作用（詳細テーブル）
    lines.append("\n## 色×条件 交互作用（logT, MixedLM）\n")

    def fit_mixedlm_logT_with_color_interaction(dfm: pd.DataFrame):
        sub = dfm.dropna(subset=["logT","n_devices_required_c","n_colors_required_c"])
        if sub.empty:
            return None
        formula = (
            "logT ~ C(condition)"
            " + n_devices_required_c"
            " + n_colors_required_c"
            " + n_devices_required_c:C(condition)"
            " + n_colors_required_c:C(condition)"
        )
        try:
            m = smf.mixedlm(formula, data=sub, groups=sub["participant"])
            return m.fit(method="lbfgs", reml=False)
        except Exception as e:
            print("[WARN] MixedLM(logT, color x condition) failed:", e)
            return None

    mix_color = fit_mixedlm_logT_with_color_interaction(dfm)
    if mix_color is None:
        lines.append("色×条件の交互作用モデルが推定できませんでした。\n")
        return

    params_fe = mix_color.fe_params
    conf_fe   = mix_color.conf_int().loc[params_fe.index]
    tmp = []
    for nm, beta in params_fe.items():
        ci_low, ci_high = conf_fe.loc[nm].values
        pct = (math.exp(beta)-1)*100
        tmp.append([nm, beta, ci_low, ci_high, pct])
    df_out = pd.DataFrame(tmp, columns=["term","beta","ci_low","ci_high","pct_change"])
    lines.append(df_out.to_string(index=False) + "\n")

    # 色数ごとの P+SR vs Pointing
    lines.append("\n### 色ごとの P+SR vs Pointing の予測時間比（<1: P+SRが速い）\n")

    def contrasts_PSR_vs_Pointing_by_colors(mix_res, dfm):
        names = mix_res.model.exog_names
        beta  = mix_res.fe_params.loc[names].values
        Vfull = mix_res.cov_params()
        V     = Vfull.loc[names, names].values

        col_vals = sorted(dfm["n_colors_required"].dropna().unique())
        rows = []

        def vec(cond, col_raw):
            col_c = col_raw - dfm["n_colors_required"].dropna().mean()
            x = np.zeros(len(names))
            if "Intercept" in names:
                x[names.index("Intercept")] = 1.0
            for n in names:
                if n.startswith("C(condition)") and f"[T.{cond}]" in n:
                    x[names.index(n)] = 1.0
            if "n_colors_required_c" in names:
                x[names.index("n_colors_required_c")] = col_c
            key = f"n_colors_required_c:C(condition)[T.{cond}]"
            if key in names:
                x[names.index(key)] = col_c
            return x

        for col_raw in col_vals:
            xa = vec("P+SR", col_raw)
            xb = vec("Pointing", col_raw)
            diff = xa - xb
            est = float(diff @ beta)
            se  = float(np.sqrt(diff @ V @ diff))
            z   = est / se if se > 0 else 0.0
            p   = 2 * (1 - norm.cdf(abs(z)))
            ratio = math.exp(est)
            rows.append([col_raw, est, se, z, p, ratio])

        out = pd.DataFrame(rows, columns=["colors","est_logT","se","z","p_raw","time_ratio_PSR/Pointing"])
        out["p_holm"] = holm_adjust(out["p_raw"].values)
        return out

    ct = contrasts_PSR_vs_Pointing_by_colors(mix_color, dfm)
    lines.append(ct.to_string(index=False) + "\n")

def write_within_corr_and_QA(lines, df_tasks: pd.DataFrame):
    lines.append("\n## 被験者内スピアマン相関（センタリング）\n")
    lines.append(within_subject_spearman(df_tasks))

    colors_unique  = sorted(df_tasks["n_colors_required"].dropna().unique().tolist()) if "n_colors_required" in df_tasks.columns else []
    devices_unique = sorted(df_tasks["n_devices_required"].dropna().unique().tolist()) if "n_devices_required" in df_tasks.columns else []
    lines.append("\n## 変動の確認\n")
    lines.append(f"- devices unique values: {devices_unique}\n")
    lines.append(f"- colors unique values:  {colors_unique}\n")

# ----------------- Optional stub -----------------
def plot_partial_effect(dfm: pd.DataFrame):
    """Optional stub to avoid NameError; fill in if you want partial effects."""
    return

# ----------------- Main -----------------
def main():
    # 1) データ読み込み & RESULTS マージ
    df_tasks, df_pc = load_inputs()

    # 2) 記述統計プロット（箱ひげ＋ECDF）
    plot_descriptive(df_pc)

    # 3) モデル用データ整形
    dfm = prepare_model_frame(df_tasks)

    # 4) Markdownレポートの準備
    lines = ["# 高度統計レポート（IMWUT向け）\n"]

    # 5) 統計セクション書き出し
    write_friedman_blocks(lines, df_pc)      # 時間・試行数・アクチュエーション・(system_time_total_sec)
    write_models_blocks(lines, dfm)          # MixedLM, GEE, 交互作用・媒介
    write_within_corr_and_QA(lines, df_tasks)# 相関とデータ品質確認

    # 6) ファイル出力
    with open(REPORT_MD, "w", encoding="utf-8") as f:
        f.write("\n".join(lines))
    print(f"[OK] wrote {REPORT_MD}")

    # 7) オプション：部分効果プロット（スタブ）
    try:
        plot_partial_effect(dfm)
    except Exception:
        pass

if __name__ == "__main__":
    main()