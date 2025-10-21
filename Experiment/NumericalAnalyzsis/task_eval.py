#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
IMWUT 論文向けの定量評価スクリプト（集計 + 可視化 + 統計検定）

前提：
- VOICE_LOG ディレクトリ配下に、参加者ごとに `./VOICE_LOG/P{n}/P{i}_*.json` という形式のファイルがある。
  - n は 1..24 の参加者ID
  - i は 条件ID（1: SR, 2: P+SR, 3: Pointing, 4: Label）
  - `*_*.json` の末尾（Aパターン）は無視して良い（全て集計対象）
- 各 JSON は配列（各要素は 1 タスク）で、以下のような構造（サマリ）
  - taskId (UUID)
  - finalId (UUID) … final attempt の ID
  - totalElapsedTime (float) … タスク全体の経過秒（無ければ最後の attempt の taskElapsedTime を採用）
  - taskAttempts: [
        { attemptId, taskElapsedTime (str/float), userCommand, outputDevices: [deviceId,...] }, ...
    ]

出力：
- ./imwut_results/task_summary.csv … タスク単位のメトリクス
- ./imwut_results/participant_condition_summary.csv … 参加者×条件のメディアン集約
- ./imwut_results/stats_report.md … 統計の要約（Friedman, ペア Wilcoxon+Holm 補正）
- ./imwut_results/plots/*.png … 論文用図版（Box+Strip / ECDF / 散布図 等）

注意：
- seaborn は使わず、matplotlib のみを使用（スタイル・色の指定もしない）。
- 無効/壊れた JSON はスキップ（エラートレラント）。

使い方：
    python imwut_task_eval.py --voice_root ./VOICE_LOG --out_root ./imwut_results

オプション：
    --participants  例: 1-24 （デフォルト）

"""

import os
import re
import glob
import json
import math
import argparse
from collections import defaultdict, OrderedDict

import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
from scipy.stats import friedmanchisquare, wilcoxon, rankdata, norm

# =============== ユーティリティ ===============

COND_MAP = {
    1: "SR",
    2: "P+SR",
    3: "Pointing",
    4: "Label",
}
COND_ORDER = ["SR", "P+SR", "Pointing", "Label"]

# ファイル無視パターン（バックアップ/一時/ごみ）—必要に応じて編集
IGNORE_PATTERNS = [
    "backup", "bak", "copy", "old", "tmp", "temp", "cache",
    "~", "#", ".swp", ".swx"
]


def is_json_file(path: str) -> bool:
    b = os.path.basename(path)
    return b.endswith(".json") and not b.endswith(".json.meta")


def should_skip_by_name(path: str) -> bool:
    b = os.path.basename(path).lower()
    for pat in IGNORE_PATTERNS:
        if pat in b:
            return True
    return False


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


def dedupe_attempts(attempts):
    """attemptId + userCommand + outputDevices が連続で同一のものを 1 回に圧縮"""
    out = []
    prev_key = None
    for a in attempts:
        aid = a.get("attemptId")
        cmd = a.get("userCommand")
        outs = a.get("outputDevices") or []
        key = (aid, cmd, tuple(outs))
        if key != prev_key:
            out.append(a)
            prev_key = key
    return out


def ecdf(series):
    x = np.array([v for v in series if pd.notna(v)])
    x.sort()
    if len(x) == 0:
        return np.array([]), np.array([])
    y = np.arange(1, len(x) + 1) / len(x)
    return x, y


# =============== ロード & 集計 ===============

def parse_voice_file(fp: str, participant: int, cond_label: str):
    """VOICE_LOG の 1 ファイル（配列: タスク集合）から、タスク単位の行を返す。
    形式/スキーマが崩れているファイルは静かにスキップ。
    """
    rows = []
    try:
        with open(fp, "r", encoding="utf-8") as f:
            arr = json.load(f)
    except Exception:
        return rows

    # 期待するトップレベルは配列
    if not isinstance(arr, list) or len(arr) == 0:
        return rows

    for task in arr:
        # スキーマの最低限チェック（ごみ JSON を除外）
        if not isinstance(task, dict):
            continue
        if "taskAttempts" not in task:
            continue
        attempts = task.get("taskAttempts")
        if not isinstance(attempts, list) or len(attempts) == 0:
            continue

        task_id = task.get("taskId")
        final_id = task.get("finalId")
        total_elapsed = safe_float(task.get("totalElapsedTime"))

        attempts = dedupe_attempts(attempts)

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

        # 最終試行で触った台数（一応保持）
        final_out = used[-1].get("outputDevices") if isinstance(used[-1], dict) else []
        if not isinstance(final_out, list):
            final_out = []
        N_devices_final = len(final_out)

        rows.append({
            "participant": participant,
            "condition": cond_label,
            "file": os.path.basename(fp),
            "taskId": task_id,
            "finalId": final_id,
            "T_task_sec": T_task,
            "N_cmds": N_cmds,
            "N_actuations": N_actuations,
            "N_devices_final": N_devices_final,
        })
    return rows

    for task in arr:
        task_id = task.get("taskId")
        final_id = task.get("finalId")
        total_elapsed = safe_float(task.get("totalElapsedTime"))
        attempts = task.get("taskAttempts") or []
        if not isinstance(attempts, list) or len(attempts) == 0:
            continue

        attempts = dedupe_attempts(attempts)

        # final index 決定
        final_idx = None
        if final_id:
            for i, a in enumerate(attempts):
                if a.get("attemptId") == final_id:
                    final_idx = i
                    break
        if final_idx is None:
            final_idx = len(attempts) - 1  # 末尾を final とみなす

        used = attempts[: final_idx + 1]

        # T_task_sec： totalElapsedTime 優先、無ければ final の taskElapsedTime
        if total_elapsed is not None:
            T_task = total_elapsed
        else:
            last_te = safe_float(used[-1].get("taskElapsedTime"))
            T_task = last_te if last_te is not None else None

        # N_cmds： final までのユニーク attemptId 数
        seen = []
        for a in used:
            aid = a.get("attemptId")
            if aid and aid not in seen:
                seen.append(aid)
        N_cmds = len(seen)

        # N_actuations： final までの outputDevices の延べ個数
        N_actuations = 0
        for a in used:
            ods = a.get("outputDevices") or []
            if isinstance(ods, list):
                N_actuations += len(ods)

        # 最終試行で触った台数（一応保持）
        final_out = used[-1].get("outputDevices") or []
        N_devices_final = len(final_out)

        rows.append({
            "participant": participant,
            "condition": cond_label,
            "file": os.path.basename(fp),
            "taskId": task_id,
            "finalId": final_id,
            "T_task_sec": T_task,
            "N_cmds": N_cmds,
            "N_actuations": N_actuations,
            "N_devices_final": N_devices_final,
        })
    return rows


def matches_condition_file(basename: str, cond_i: int) -> bool:
    """ファイル名から条件 i (1..4) に対応しているかを判定。
    許可例: "P1_*.json" / "1_*.json" / "P1-*.json" / "1-*.json" （大文字小文字無視）
    """
    b = basename.lower()
    # 拡張子は .json 前提（.json.meta は is_json_file で除外済み）
    return bool(re.match(rf"^(p?{cond_i})[\-_].*\.json$", b))


def collect_tasks(voice_root: str, p_start: int, p_end: int):
    all_rows = []
    scanned_files = 0
    used_files = 0
    for p in range(p_start, p_end + 1):
        pdir = os.path.join(voice_root, f"P{p}")
        if not os.path.isdir(pdir):
            continue
        # 再帰的に .json を探索（.json.meta は除外）
        candidates = [
            fp for fp in glob.glob(os.path.join(pdir, "**", "*.json"), recursive=True)
            if is_json_file(fp) and not should_skip_by_name(fp)
        ]
        scanned_files += len(candidates)
        for i in [1, 2, 3, 4]:
            cond = COND_MAP[i]
            for fp in candidates:
                b = os.path.basename(fp)
                if not matches_condition_file(b, i):
                    continue
                used_files += 1
                all_rows.extend(parse_voice_file(fp, p, cond))
    if scanned_files == 0:
        print(f"[INFO] VOICE_LOG 内で .json ファイルが見つかりませんでした: {voice_root}")
    else:
        print(f"[INFO] 走査ファイル数: {scanned_files} / 集計対象に採用: {used_files}")
    return pd.DataFrame(all_rows)


# =============== 統計 ===============

def robust_aggregate(df_tasks: pd.DataFrame) -> pd.DataFrame:
    """参加者×条件でメディアン集約（各メトリクス）"""
    if df_tasks.empty:
        return df_tasks
    cols = ["T_task_sec", "N_cmds", "N_actuations", "N_devices_final"]
    g = df_tasks.groupby(["participant", "condition"], as_index=False)[cols].median()
    return g


def friedman_within_subject(df_pc: pd.DataFrame, metric: str):
    """Friedman 検定（被験者内、4条件）。
    df_pc: participant-condition テーブル
    return: (stat, p, wide_df)
    """
    # wide にする：行=participant、列=条件
    pivot = df_pc.pivot(index="participant", columns="condition", values=metric)
    pivot = pivot.reindex(columns=COND_ORDER)
    # 欠損がある参加者は除外（ペアデザイン）
    pivot = pivot.dropna(axis=0, how="any")
    arrays = [pivot[c].values for c in COND_ORDER]
    if len(pivot) == 0:
        return None, None, pivot
    stat, p = friedmanchisquare(*arrays)
    return stat, p, pivot


def pairwise_wilcoxon_holm(pivot: pd.DataFrame, metric: str):
    """列名（条件）間でのペア Wilcoxon（対応あり）。Holm 補正を実装。
    戻り値：DataFrame（pair, n, W, p_raw, p_holm, effect_r）
    """
    conds = [c for c in pivot.columns if c in COND_ORDER]
    pairs = []
    from itertools import combinations
    alpha = 0.05
    pvals = []
    tmp_res = []
    for a, b in combinations(conds, 2):
        x = pivot[a].values
        y = pivot[b].values
        # 同値ばかりだとエラーになることがあるので try
        try:
            stat, p = wilcoxon(x, y, zero_method='wilcox', correction=False)
        except Exception:
            # 差が全て0等のケース → p=1.0 扱い
            stat, p = (0.0, 1.0)
        # 効果量 r ≈ |Z|/sqrt(N) の近似（Z 近似：正規近似）
        # Wilcoxon の W を Z に近似するには、平均と分散を使う
        n = len(x)
        # 同順位補正等は簡略化（IMWUT補助資料では詳細化も可）
        mean_W = n * (n + 1) / 4.0
        var_W = n * (n + 1) * (2 * n + 1) / 24.0
        if var_W > 0:
            z = (stat - mean_W) / math.sqrt(var_W)
            r = abs(z) / math.sqrt(n)
        else:
            r = 0.0
        tmp_res.append((f"{a} vs {b}", n, stat, p, r))
        pvals.append(p)

    # Holm 補正
    m = len(pvals)
    order = np.argsort(pvals)
    adjusted = [None] * m
    prev = 0.0
    for k, idx in enumerate(order):
        p = pvals[idx]
        adj = (m - k) * p
        adj = max(adj, prev)  # monotone
        adj = min(adj, 1.0)
        adjusted[idx] = adj
        prev = adj

    out_rows = []
    for (pair, n, W, p_raw, r), p_holm in zip(tmp_res, adjusted):
        out_rows.append({
            "pair": pair,
            "n": n,
            "W": W,
            "p_raw": p_raw,
            "p_holm": p_holm,
            "effect_r": r,
        })
    return pd.DataFrame(out_rows)


# =============== 可視化 ===============

def strip_box(ax, values_by_group, labels, title, ylabel):
    # ボックス
    ax.boxplot(values_by_group, labels=labels, showfliers=False)
    # ストリップ（ばら撒き）
    for i, vals in enumerate(values_by_group, start=1):
        if vals is None:
            continue
        x = np.random.normal(loc=i, scale=0.08, size=len(vals))
        ax.plot(x, vals, 'o', alpha=0.6, markersize=3)
    ax.set_title(title)
    ax.set_ylabel(ylabel)
    ax.grid(alpha=0.3)


def plot_boxstrip(df_pc: pd.DataFrame, metric: str, out_path: str):
    # 条件順で値を並べる
    vals = []
    for c in COND_ORDER:
        arr = df_pc[df_pc["condition"] == c][metric].dropna().values
        vals.append(arr)
    fig, ax = plt.subplots(figsize=(6.5, 4.2))
    strip_box(ax, vals, COND_ORDER, f"{metric} by Condition", metric)
    fig.tight_layout()
    fig.savefig(out_path, dpi=220)
    plt.close(fig)


def plot_ecdf_by_condition(df_pc: pd.DataFrame, metric: str, out_path: str):
    fig, ax = plt.subplots(figsize=(6.5, 4.2))
    for c in COND_ORDER:
        arr = df_pc[df_pc["condition"] == c][metric]
        x, y = ecdf(arr)
        if len(x) == 0:
            continue
        ax.plot(x, y, label=c)
    ax.set_title(f"ECDF of {metric} by Condition")
    ax.set_xlabel(metric)
    ax.set_ylabel("F(x)")
    ax.legend()
    ax.grid(alpha=0.3)
    fig.tight_layout()
    fig.savefig(out_path, dpi=220)
    plt.close(fig)


def plot_scatter(df_pc: pd.DataFrame, x_col: str, y_col: str, out_path: str):
    fig, ax = plt.subplots(figsize=(6.2, 4.2))
    ax.plot(df_pc[x_col], df_pc[y_col], 'o', alpha=0.6)
    ax.set_xlabel(x_col)
    ax.set_ylabel(y_col)
    ax.set_title(f"{y_col} vs {x_col}")
    ax.grid(alpha=0.3)
    fig.tight_layout()
    fig.savefig(out_path, dpi=220)
    plt.close(fig)


# =============== メイン ===============

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--voice_root", required=True, help="VOICE_LOG のルートパス")
    ap.add_argument("--out_root", required=True, help="出力ルートディレクトリ")
    ap.add_argument("--participants", default="1-24", help="例: 1-24 または 3,7,9")
    args = ap.parse_args()

    # 出力ディレクトリ
    out_root = args.out_root
    os.makedirs(out_root, exist_ok=True)
    plot_dir = os.path.join(out_root, "plots")
    os.makedirs(plot_dir, exist_ok=True)

    # 参加者リスト
    if "," in args.participants:
        plist = [int(x) for x in args.participants.split(",")]
    elif "-" in args.participants:
        s, e = args.participants.split("-")
        plist = list(range(int(s), int(e) + 1))
    else:
        plist = [int(args.participants)]

    # === タスク単位の収集 ===
    df_tasks = collect_tasks(args.voice_root, min(plist), max(plist))
    if df_tasks.empty:
        print("[WARN] タスクデータが見つかりません。パスやファイル形式を確認してください。")
        return

    # CSV 保存
    tasks_csv = os.path.join(out_root, "task_summary.csv")
    df_tasks.to_csv(tasks_csv, index=False)

    # === 参加者×条件でメディアン集約 ===
    df_pc = robust_aggregate(df_tasks)
    pc_csv = os.path.join(out_root, "participant_condition_summary.csv")
    df_pc.to_csv(pc_csv, index=False)

    # === 可視化 ===
    for metric in ["T_task_sec", "N_cmds", "N_actuations"]:
        plot_boxstrip(df_pc, metric, os.path.join(plot_dir, f"boxstrip_{metric}.png"))
        plot_ecdf_by_condition(df_pc, metric, os.path.join(plot_dir, f"ecdf_{metric}.png"))

    # 相関的な眺め（任意）
    if set(["T_task_sec", "N_cmds"]).issubset(df_pc.columns):
        plot_scatter(df_pc, "N_cmds", "T_task_sec", os.path.join(plot_dir, "scatter_T_vs_cmds.png"))

    # === 統計：Friedman + ペア Wilcoxon (Holm補正) ===
    lines = []
    lines.append("# 統計レポート\n")

    for metric in ["T_task_sec", "N_cmds", "N_actuations"]:
        stat, p, pivot = friedman_within_subject(df_pc, metric)
        lines.append(f"\n## {metric}\n")
        if stat is None:
            lines.append("有効なデータが不足しています。\n")
            continue
        lines.append(f"Friedman: χ²={stat:.4f}, p={p:.6g}, N={len(pivot)}\n")

        # 記述統計（条件別）
        lines.append("\n### 記述統計（参加者×条件のメディアン単位）\n")
        desc_rows = []
        for c in COND_ORDER:
            vals = pivot[c].dropna().values
            if len(vals) == 0:
                desc_rows.append((c, np.nan, np.nan, np.nan, np.nan, 0))
            else:
                med = np.median(vals)
                q1 = np.percentile(vals, 25)
                q3 = np.percentile(vals, 75)
                mu = np.mean(vals)
                sd = np.std(vals, ddof=1) if len(vals) > 1 else 0.0
                desc_rows.append((c, med, q1, q3, mu, sd))
        lines.append("Condition | Median | Q1 | Q3 | Mean | SD")
        lines.append("---|---:|---:|---:|---:|---:")
        for c, med, q1, q3, mu, sd in desc_rows:
            lines.append(f"{c} | {med:.3g} | {q1:.3g} | {q3:.3g} | {mu:.3g} | {sd:.3g}")

        # 有意ならペア検定
        if p is not None and p < 0.05:
            df_pairs = pairwise_wilcoxon_holm(pivot, metric)
            lines.append("\n### ペアごとの Wilcoxon（Holm 補正）\n")
            lines.append("Pair | n | W | p_raw | p_holm | effect_r")
            lines.append("---|---:|---:|---:|---:|---:")
            for _, r in df_pairs.iterrows():
                lines.append(f"{r['pair']} | {int(r['n'])} | {r['W']:.3g} | {r['p_raw']:.3g} | {r['p_holm']:.3g} | {r['effect_r']:.3g}")
        else:
            lines.append("\n（Friedman で有意差なし → ペア検定は省略）\n")

    # レポート保存
    report_md = os.path.join(out_root, "stats_report.md")
    with open(report_md, "w", encoding="utf-8") as f:
        f.write("\n".join(lines) + "\n")

    print("=== 完了 ===")
    print(f"Tasks CSV: {tasks_csv}")
    print(f"Participant×Condition CSV: {pc_csv}")
    print(f"Stats report: {report_md}")
    print(f"Plots: {plot_dir}")


if __name__ == "__main__":
    main()
