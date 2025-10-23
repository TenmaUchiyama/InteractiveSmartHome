#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
Evaluator (all-in-one)

- VOICE_LOG を走査し、taskId を arrange_color_map のキーとして色の種類数を付与
- タスク単位 CSV と 参加者×条件メディアン CSV を出力
- ご指定フォーマットの stats_report.md を生成（Friedman→有意なら Wilcoxon+Holm）

使い方:
    python evaluator.py
"""

import os
import re
import glob
import json
import math
from typing import Any, Dict, List

import numpy as np
import pandas as pd
from scipy.stats import friedmanchisquare, wilcoxon

# ====== パス設定 ======
VOICE_ROOT = "./VOICE_LOG"
ARRANGE_COLOR_MAP_PATH = "./arrange_color_map.json"
OUT_DIR = "./imwut_results"
os.makedirs(OUT_DIR, exist_ok=True)

# 条件番号 → ラベル
COND_MAP = {1: "SR", 2: "P+SR", 3: "Pointing", 4: "Label"}
COND_ORDER = ["SR", "P+SR", "Pointing", "Label"]

# =============== ユーティリティ ===============
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

def dedupe_attempts(attempts: List[Dict[str, Any]]) -> List[Dict[str, Any]]:
    """attemptId + userCommand + outputDevices が連続で同一のものを 1 回に圧縮"""
    out = []
    prev_key = None
    for a in attempts:
        if not isinstance(a, dict):
            continue
        aid = a.get("attemptId")
        cmd = a.get("userCommand")
        outs = a.get("outputDevices") or []
        key = (aid, cmd, tuple(outs) if isinstance(outs, list) else tuple())
        if key != prev_key:
            out.append(a)
            prev_key = key
    return out

def parse_condition_from_filename(basename: str):
    """
    先頭の数字(1..4)を条件番号として解釈（例: '3_C.json', '2-B.json', '1.json'）
    """
    m = re.match(r"^\s*([1-4])(?:[\-_].*)?\.json$", basename, flags=re.IGNORECASE)
    if not m:
        return None
    return int(m.group(1))

def load_arrange_color_map(path: str) -> Dict[str, List[str]]:
    try:
        with open(path, "r", encoding="utf-8") as f:
            m = json.load(f)
        # 値を必ずリストに正規化
        norm = {}
        for k, v in m.items():
            if isinstance(v, list):
                norm[k] = v
            elif v is None:
                norm[k] = []
            else:
                norm[k] = [str(v)]
        return norm
    except Exception as e:
        print(f"[WARN] arrange_color_map 読み込み失敗: {e}")
        return {}

# =============== 集計 ===============
def collect_rows(voice_root: str, arrange_color_map: Dict[str, List[str]]):
    rows = []
    pdirs = [d for d in sorted(os.listdir(voice_root)) if d.startswith("P") and not d.endswith(".meta")]
    for pdn in pdirs:
        try:
            participant_id = int(pdn[1:])  # 'P12' -> 12
        except Exception:
            continue
        pdir = os.path.join(voice_root, pdn)
        json_files = [fp for fp in glob.glob(os.path.join(pdir, "*.json")) if is_json_file(fp)]

        for fp in json_files:
            b = os.path.basename(fp)
            cond_num = parse_condition_from_filename(b)
            if cond_num is None or cond_num not in COND_MAP:
                continue
            condition = COND_MAP[cond_num]

            # ファイル読み込み
            try:
                with open(fp, "r", encoding="utf-8") as f:
                    data = json.load(f)
            except Exception:
                continue
            if not isinstance(data, list) or len(data) == 0:
                continue

            for task in data:
                if not isinstance(task, dict):
                    continue
                attempts = task.get("taskAttempts")
                if not isinstance(attempts, list) or len(attempts) == 0:
                    continue

                # 重複圧縮（任意）
                attempts = dedupe_attempts(attempts)

                task_id = task.get("taskId")  # == device_arrange_id と同義（ユーザー定義）
                final_id = task.get("finalId")
                total_elapsed = safe_float(task.get("totalElapsedTime"))

                # final index 決定
                final_idx = None
                if final_id:
                    for i, a in enumerate(attempts):
                        if isinstance(a, dict) and a.get("attemptId") == final_id:
                            final_idx = i
                            break
                if final_idx is None:
                    final_idx = len(attempts) - 1  # 末尾を final とみなす
                used = attempts[: final_idx + 1]

                # T_task_sec： totalElapsedTime 優先、無ければ final の taskElapsedTime
                if total_elapsed is not None:
                    T_task = total_elapsed
                else:
                    last_te = safe_float(used[-1].get("taskElapsedTime") if isinstance(used[-1], dict) else None)
                    T_task = last_te if last_te is not None else None

                # N_cmds： final までのユニーク attemptId 数
                seen = []
                for a in used:
                    if not isinstance(a, dict):
                        continue
                    aid = a.get("attemptId")
                    if aid and aid not in seen:
                        seen.append(aid)
                N_cmds = len(seen)

                # N_actuations： final までの outputDevices の延べ個数
                N_actuations = 0
                for a in used:
                    if not isinstance(a, dict):
                        continue
                    ods = a.get("outputDevices") or []
                    if isinstance(ods, list):
                        N_actuations += len(ods)

                # 最終試行で触った台数（参考）
                final_out = used[-1].get("outputDevices") if isinstance(used[-1], dict) else []
                if not isinstance(final_out, list):
                    final_out = []
                n_devices_final = len(final_out)

                # 色の種類数: taskId を arrange_color_map のキーとして直接引く
                if task_id and task_id in arrange_color_map:
                    n_colors_required = len(set(arrange_color_map[task_id]))
                else:
                    n_colors_required = np.nan  # 見つからなければ NaN

                rows.append({
                    "participant": participant_id,
                    "condition": condition,
                    "file": b,
                    "taskId": task_id,
                    "T_task_sec": T_task,
                    "N_cmds": N_cmds,
                    "N_actuations": N_actuations,
                    "n_devices_final": n_devices_final,
                    "n_colors_required": n_colors_required,
                })
    return rows

# =============== 統計（レポート生成） ===============
def friedman_within_subject(df_pc: pd.DataFrame, metric: str):
    """Friedman 検定（被験者内、4条件）。
    return: (stat, p, pivot_wide)
    """
    pivot = df_pc.pivot(index="participant", columns="condition", values=metric)
    pivot = pivot.reindex(columns=COND_ORDER)
    pivot = pivot.dropna(axis=0, how="any")  # 欠損のある参加者は除外
    arrays = [pivot[c].values for c in COND_ORDER]
    if len(pivot) == 0:
        return None, None, pivot
    stat, p = friedmanchisquare(*arrays)
    return stat, p, pivot

def pairwise_wilcoxon_holm(pivot: pd.DataFrame):
    """列名（条件）間でのペア Wilcoxon（対応あり）。Holm 補正。
    戻り値：DataFrame（pair, n, W, p_raw, p_holm, effect_r）
    """
    conds = [c for c in pivot.columns if c in COND_ORDER]
    from itertools import combinations
    pvals = []
    tmp_res = []
    for a, b in combinations(conds, 2):
        x = pivot[a].values
        y = pivot[b].values
        try:
            W, p = wilcoxon(x, y, zero_method='wilcox', correction=False)
        except Exception:
            W, p = (0.0, 1.0)
        n = len(x)
        mean_W = n * (n + 1) / 4.0
        var_W  = n * (n + 1) * (2 * n + 1) / 24.0
        if var_W > 0:
            z = (W - mean_W) / math.sqrt(var_W)
            r = abs(z) / math.sqrt(n)
        else:
            r = 0.0
        tmp_res.append((f"{a} vs {b}", n, W, p, r))
        pvals.append(p)

    # Holm
    m = len(pvals)
    order = np.argsort(pvals)
    adjusted = [None] * m
    prev = 0.0
    for k, idx in enumerate(order):
        adj = (m - k) * pvals[idx]
        adj = max(adj, prev)
        adj = min(adj, 1.0)
        adjusted[idx] = adj
        prev = adj

    out_rows = []
    for (pair, n, W, p_raw, r), p_holm in zip(tmp_res, adjusted):
        out_rows.append({
            "pair": pair, "n": n, "W": W,
            "p_raw": p_raw, "p_holm": p_holm, "effect_r": r
        })
    return pd.DataFrame(out_rows)

def descr_block(pivot: pd.DataFrame) -> str:
    """条件別の記述統計（Median, Q1, Q3, Mean, SD）→ Markdown"""
    lines = []
    lines.append("\n### 記述統計（参加者×条件のメディアン単位）\n")
    lines.append("Condition | Median | Q1 | Q3 | Mean | SD")
    lines.append("---|---:|---:|---:|---:|---:")
    for c in COND_ORDER:
        vals = pivot[c].dropna().values if c in pivot.columns else np.array([])
        if len(vals) == 0:
            lines.append(f"{c} |  |  |  |  | ")
            continue
        med = np.median(vals)
        q1  = np.percentile(vals, 25)
        q3  = np.percentile(vals, 75)
        mu  = np.mean(vals)
        sd  = np.std(vals, ddof=1) if len(vals) > 1 else 0.0
        lines.append(f"{c} | {med:.3g} | {q1:.3g} | {q3:.3g} | {mu:.3g} | {sd:.3g}")
    return "\n".join(lines) + "\n"

def pairs_block(pivot: pd.DataFrame, metric: str, p_friedman: float) -> str:
    """Friedman有意のときのみ Wilcoxon（Holm 補正）を出力"""
    if p_friedman is None or p_friedman >= 0.05:
        return "\n（Friedman で有意差なし → ペア検定は省略）\n"
    df_pairs = pairwise_wilcoxon_holm(pivot)
    lines = []
    lines.append("\n### ペアごとの Wilcoxon（Holm 補正）\n")
    lines.append("Pair | n | W | p_raw | p_holm | effect_r")
    lines.append("---|---:|---:|---:|---:|---:")
    for _, r in df_pairs.iterrows():
        lines.append(f"{r['pair']} | {int(r['n'])} | {r['W']:.3g} | {r['p_raw']:.3g} | {r['p_holm']:.3g} | {r['effect_r']:.3g}")
    return "\n".join(lines) + "\n"

def make_stats_report(df_pc: pd.DataFrame, out_path: str,
                      metrics=("T_task_sec","N_cmds","N_actuations")):
    """指定フォーマットで Markdown レポートを書き出し"""
    lines = []
    lines.append("# 統計レポート\n")

    for metric in metrics:
        stat, p, pivot = friedman_within_subject(df_pc, metric)
        lines.append(f"\n## {metric}\n")
        if stat is None:
            lines.append("有効なデータが不足しています。\n")
            continue
        lines.append(f"\nFriedman: χ²={stat:.4f}, p={p:.6g}, N={len(pivot)}\n")
        lines.append(descr_block(pivot))
        lines.append(pairs_block(pivot, metric, p))

    with open(out_path, "w", encoding="utf-8") as f:
        f.write("\n".join(lines))
    print(f"[OK] stats report written -> {out_path}")

# =============== メイン ===============
def main():
    # --- arrange_color_map のロード
    arrange_color_map = load_arrange_color_map(ARRANGE_COLOR_MAP_PATH)

    # --- タスク行収集
    rows = collect_rows(VOICE_ROOT, arrange_color_map)
    if not rows:
        print("[WARN] 取り出せた行が 0 件です。パスやファイル名形式、JSONの中身を確認してください。")
        return

    df_tasks = pd.DataFrame(rows)
    tasks_csv = os.path.join(OUT_DIR, "task_summary_enriched.csv")
    df_tasks.to_csv(tasks_csv, index=False)
    print(f"[OK] wrote {tasks_csv}  ({len(df_tasks)} rows)")

    # --- 参加者×条件メディアン集計
    cols = ["T_task_sec", "N_cmds", "N_actuations", "n_devices_final", "n_colors_required"]
    df_pc = df_tasks.groupby(["participant", "condition"], as_index=False)[cols].median(numeric_only=True)
    pc_csv = os.path.join(OUT_DIR, "participant_condition_summary.csv")
    df_pc.to_csv(pc_csv, index=False)
    print(f"[OK] wrote {pc_csv}  ({len(df_pc)} rows)")

    # --- 簡易の条件別記述統計を標準出力
    desc = (
        df_pc.groupby("condition")[["T_task_sec","N_cmds","N_actuations","n_devices_final","n_colors_required"]]
        .agg(["median","mean","std","count"])
        .reindex(COND_ORDER)
    )
    print("\n=== Condition-wise summary (participant×condition median basis) ===")
    print(desc)

    # --- 統計レポート（ご指定フォーマット）
    report_md = os.path.join(OUT_DIR, "stats_report.md")
    make_stats_report(df_pc, report_md, metrics=("T_task_sec","N_cmds","N_actuations"))

if __name__ == "__main__":
    main()
