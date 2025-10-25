# export_summary.py
# -*- coding: utf-8 -*-
"""
SurvayResult.csv と Analyzer/analyzer.py を使って、
主要な統計結果を 1 つのテキストにまとめて outputs/summary.txt として保存します。
オプションで、ユーザ好みの順位データ（日本語ヘッダ）から Bradley–Terry も計算可能。
さらに、すべての表を 1 枚の CSV (outputs/summary_all.csv) に縦結合で集約します。

配置想定(既定):
  ./ResultData/SurvayResult.csv
  ./Analyzer/analyzer.py
  ./Analyzer/dataset.py
  ./export_summary.py

実行例:
  python export_summary.py
  python export_summary.py --csv ./SurvayResult.csv --out ./outputs --pref ./ResultData/Preference.csv
"""

from __future__ import annotations
import os
import sys
import math
import argparse
import textwrap
from datetime import datetime

import pandas as pd

# ========== 既定パス ==========
DEFAULT_CSV = "./ResultData/SurvayResult.csv"
DEFAULT_OUTDIR = "./outputs"
DEFAULT_PREF = None  # 例: "./ResultData/Preference.csv" があれば渡す
DEFAULT_OUTCSV = "summary_all.csv"

# 単一CSVへ集約するテーブルのバッファ
CSV_TABLES: list[pd.DataFrame] = []

# ========== Analyzer へのパス通し ==========
BASE = os.path.abspath(os.getcwd())
AN_DIR = os.path.join(BASE, "Analyzer")
if AN_DIR not in sys.path:
    sys.path.insert(0, AN_DIR)

try:
    from analyzer import SurveyResultAnalyzer
except Exception as e:
    raise RuntimeError("Analyzer/analyzer.py の読み込みに失敗しました。依存ファイルやパスを確認してください。") from e

# ========== ユーティリティ ==========
ABBR = {
    "SpatialReference": "SR",
    "SpatialReference+Pointing": "P+SR",
    "Pointing": "Pointing",
    "Label": "Label",
}

def fmt_p(p):
    if p is None or (isinstance(p, float) and (math.isnan(p) or math.isinf(p))):
        return "n/a"
    if p < 1e-3:
        return "< .001"
    return f"{p:.3f}"

def fmt_ci(ci):
    if not ci or not isinstance(ci, (tuple, list)) or len(ci) != 2:
        return "n/a"
    return f"[{ci[0]:.2f}, {ci[1]:.2f}]"

def write_line(f, s=""):
    f.write(s + "\n")

def write_section_header(f, title):
    write_line(f, "=" * len(title))
    write_line(f, title)
    write_line(f, "=" * len(title))

def describe_block_to_lines(desc_dict, want_ci=True):
    """
    desc_dict: { ConditionName: {"mean":..,"std":..,"median":..,"iqr":..,"n":.., "ci95":(lo,hi)?} }
    """
    lines = []
    header = f"{'Cond':<8} {'Median':>8} {'IQR':>8} {'Mean':>8} {'SD':>8} {'n':>4} {'95%CI(med)':>16}"
    lines.append(header)
    lines.append("-" * len(header))
    # 固定順で見やすく
    order = ["SpatialReference", "SpatialReference+Pointing", "Pointing", "Label"]
    for cond in order:
        if cond not in desc_dict:
            continue
        stats = desc_dict[cond]
        med   = stats.get("median", float("nan"))
        iqr   = stats.get("iqr", float("nan"))
        mean  = stats.get("mean", float("nan"))
        sd    = stats.get("std", float("nan"))
        n     = stats.get("n", 0)
        ci    = stats.get("ci95") if want_ci else None
        lines.append(f"{ABBR.get(cond, cond):<8} {med:>8.2f} {iqr:>8.2f} {mean:>8.2f} {sd:>8.2f} {n:>4} {fmt_ci(ci):>16}")
    return lines

def safe_describe(fn, with_ci=True):
    # analyzer.describe_* が with_ci 未対応でも動くようにフォールバック
    try:
        return fn(with_ci=with_ci), with_ci
    except TypeError:
        return fn(), False

def add_table(section: str, name: str, df: pd.DataFrame, **meta):
    """
    単一CSVへ縦結合するため、各テーブルに section / table などのメタ列を付与。
    """
    if df is None or len(df) == 0:
        return
    d2 = df.copy()
    d2.insert(0, "section", section)
    d2.insert(1, "table", name)
    for k, v in meta.items():
        d2[k] = v
    CSV_TABLES.append(d2)

def desc_dict_to_df(desc_dict: dict, scale: str, subscale: str, want_ci: bool=True) -> pd.DataFrame:
    """
    desc_dict: { ConditionName: {"mean","std","median","iqr","n","ci95":(lo,hi)?}, ... }
    """
    order = ["SpatialReference", "SpatialReference+Pointing", "Pointing", "Label"]
    rows = []
    for cond in order:
        s = desc_dict.get(cond)
        if not s:
            continue
        ci = s.get("ci95") if want_ci else None
        ci_lo, ci_hi = (ci if (isinstance(ci, (tuple, list)) and len(ci) == 2) else (None, None))
        rows.append({
            "scale": scale,
            "subscale": subscale,
            "condition": ABBR.get(cond, cond),
            "median": s.get("median"),
            "iqr": s.get("iqr"),
            "mean": s.get("mean"),
            "sd": s.get("std"),
            "n": s.get("n"),
            "ci_low_med": ci_lo,
            "ci_high_med": ci_hi,
        })
    return pd.DataFrame(rows)

def melt_nemenyi_table(ptab: pd.DataFrame, scale: str, subscale: str) -> pd.DataFrame:
    # 略称へ置換（表示用）
    ptab2 = ptab.rename(index=ABBR).rename(columns=ABBR).copy()

    # reset_index 後の index 列名を安全に取得
    # （index に名前が無い場合は 'index' が列名になる）
    raw_idx_name = ptab2.index.name or "index"

    # インデックスを列にして、まず 'row_cond' にリネーム
    df = ptab2.reset_index().rename(columns={raw_idx_name: "row_cond"})

    # ロング化
    long = df.melt(id_vars="row_cond", var_name="col_cond", value_name="p_adj")
    long["scale"] = scale
    long["subscale"] = subscale
    return long

# --------- Bradley–Terry（任意の順位CSVが渡された場合）---------
def run_bradley_terry(pref_csv: str):
    """
    日本語ヘッダ:
      '1番使いやすかったもの','2番目に使いやすかったもの','3番目に使いやすかったもの','4番目に使いやすかったもの'
    値は: '空間参照だけ','Pointing + 空間参照','Pointing','Label' を想定
    """
    try:
        import numpy as np
        import choix  # pip install choix
    except Exception as e:
        return f"(Bradley–Terry をスキップ: ライブラリが見つかりません: {e})", {}

    if not os.path.exists(pref_csv):
        return f"(Bradley–Terry をスキップ: ファイルが見つかりません: {pref_csv})", {}

    df = pd.read_csv(pref_csv)

    COL_R1 = "1番使いやすかったもの"
    COL_R2 = "2番目に使いやすかったもの"
    COL_R3 = "3番目に使いやすかったもの"
    COL_R4 = "4番目に使いやすかったもの"

    LABEL_MAP = {
        "空間参照だけ": "SR",
        "Pointing + 空間参照": "P+SR",
        "Pointing": "Pointing",
        "Label": "Label",
    }

    def norm(x: str) -> str:
        if isinstance(x, str):
            x = x.strip()
            return LABEL_MAP.get(x, x)
        return x

    # ランキング配列（左ほど好ましい）
    rankings = []
    for _, r in df.iterrows():
        rnk = [norm(r.get(COL_R1)), norm(r.get(COL_R2)), norm(r.get(COL_R3)), norm(r.get(COL_R4))]
        if any(pd.isna(x) for x in rnk):
            continue
        uniq = set(rnk)
        if len(uniq) == 4 and uniq.issubset({"SR", "P+SR", "Pointing", "Label"}):
            rankings.append(rnk)
    if not rankings:
        return "(Bradley–Terry をスキップ: ランキングデータが空です。列名や値を確認してください)", {}

    COND_ORDER = ["SR", "P+SR", "Pointing", "Label"]
    idx = {c: i for i, c in enumerate(COND_ORDER)}

    def pairs_from_ranking(rnk):
        out = []
        for i in range(len(rnk)):
            for j in range(i + 1, len(rnk)):
                out.append((rnk[i], rnk[j]))  # 左 > 右
        return out

    # 観測勝敗を構築（診断・出力用）
    K = len(COND_ORDER)
    counts = np.zeros((K, K), dtype=int)  # counts[w, l]: w が l に勝った回数
    edges = []
    for rnk in rankings:
        for (w, l) in pairs_from_ranking(rnk):
            wi, li = idx[w], idx[l]
            counts[wi, li] += 1
            edges.append((wi, li))

    # --- 推定（スムージング付きで堅牢化） ---
    def fit_with_smoothing(edges, max_boost=3):
        """
        reducible（強連結でない）なとき、全ペアに両方向の擬似エッジを少量足す。
        max_boost回まで（1→2→3）と増やしてリトライ。
        """
        base_edges = list(edges)
        for boost in range(0, max_boost + 1):
            try:
                if boost == 0:
                    theta = choix.ilsr_pairwise(K, base_edges)
                else:
                    aug = list(base_edges)
                    # 両方向に 'boost' 回ずつ擬似試合を追加
                    for i in range(K):
                        for j in range(K):
                            if i == j:
                                continue
                            aug.extend([(i, j)] * boost)
                    theta = choix.ilsr_pairwise(K, aug)
                return np.array(theta) - np.mean(theta), boost
            except Exception:
                continue
        raise RuntimeError("BT推定に失敗（強連結化しても収束せず）")

    theta, used_boost = fit_with_smoothing(edges, max_boost=3)
    ex = np.exp(theta - np.max(theta))
    probs = ex / ex.sum()
    beta   = {COND_ORDER[i]: float(theta[i]) for i in range(K)}
    scores = {COND_ORDER[i]: float(probs[i]) for i in range(K)}

    # 参考のAIC/BIC（擬似エッジは尤度に含めない）
    def _ll(th):
        s = 0.0
        for (w, l) in edges:
            s += math.log(1.0 / (1.0 + math.exp(th[l] - th[w])))
        return s
    ll = _ll(theta); k_param = K - 1
    AIC = -2 * ll + 2 * k_param
    BIC = -2 * ll + k_param * math.log(len(edges))

    # テキスト出力
    lines = []
    lines.append("Preference (Bradley–Terry)")
    lines.append("-" * len(lines[-1]))
    lines.append(f"n_participants(valid): {len(rankings)}")
    lines.append(f"AIC={AIC:.2f}, BIC={BIC:.2f} (参考)")
    if used_boost > 0:
        lines.append(f"※ 強連結化のため擬似エッジ（両方向×{used_boost}）を全ペアに追加しました。")

    lines.append("\nScores (softmax; 和=1, 高いほど好まれる):")
    for k in COND_ORDER:
        lines.append(f"  {k:8s}: {scores[k]:.3f}")

    lines.append("\nBeta (平均0基準):")
    for k in COND_ORDER:
        lines.append(f"  {k:8s}: {beta[k]:.3f}")

    # 観測勝敗の表
    lines.append("\nObserved wins (rows beat cols):")
    hdr = "          " + "".join([f"{c:>10}" for c in COND_ORDER])
    lines.append(hdr)
    for i, rlab in enumerate(COND_ORDER):
        row = "  " + f"{rlab:<8}" + "".join([f"{counts[i, j]:>10d}" for j in range(K)])
        lines.append(row)

    lines.append("\nRanking by score:")
    for i, (k, v) in enumerate(sorted(scores.items(), key=lambda kv: kv[1], reverse=True), start=1):
        lines.append(f"  {i}. {k} ({v:.3f})")

    # DF群
    df_scores = pd.DataFrame([{"condition": k, "score": scores[k]} for k in COND_ORDER])
    df_beta   = pd.DataFrame([{"condition": k, "beta": beta[k]} for k in COND_ORDER])
    wins_long = [{"row_cond": COND_ORDER[i], "col_cond": COND_ORDER[j], "wins": int(counts[i, j])}
                 for i in range(K) for j in range(K)]
    df_wins = pd.DataFrame(wins_long)
    dfs = {
        "bt_scores": df_scores,
        "bt_beta": df_beta,
        "bt_wins_matrix": df_wins,
        "bt_info": pd.DataFrame([{
            "n_participants_valid": len(rankings),
            "AIC": AIC, "BIC": BIC, "used_smoothing_boost": used_boost
        }]),
    }
    return "\n".join(lines), dfs

def friedman_block_with_csv(f, analyzer, scale, subscale, save_dir=None, prefix=""):
    """
    Friedman + Nemenyi をテキスト出力しつつ、単一CSV集約にも追加。
    """
    res = analyzer.friedman_nemenyi(scale=scale, subscale=subscale)
    fr  = res["friedman"]
    chi2, df, p, W, n, k = fr["chi2"], fr["df"], fr["p"], fr["W"], fr["n"], fr["k"]

    # CSV: Friedman summary
    add_table("Inferential", "friedman_summary", pd.DataFrame([{
        "scale": scale, "subscale": subscale,
        "chi2": chi2, "df": df, "p": p, "kendalls_W": W, "n": n, "k": k
    }]))

    # テキスト出力
    write_line(f, f"- Friedman: χ²({df}) = {chi2:.3f}, p = {fmt_p(p)}, Kendall's W = {W:.3f}, n = {n}, k = {k}")

    # CSV: Nemenyi（ロング形式）
    ptab = res["nemenyi"].copy()
    add_table("Inferential", "nemenyi_adjusted_p", melt_nemenyi_table(ptab, scale, subscale))

    # テキスト：Nemenyi 行列表
    ptab_txt = ptab.rename(index=ABBR).rename(columns=ABBR)
    write_line(f, "  Adjusted p-values (Nemenyi):")
    cols = list(ptab_txt.columns)
    hdr = "    " + "".join([f"{c:>10}" for c in cols])
    write_line(f, hdr)
    for idx, row in ptab_txt.iterrows():
        line = f"    {idx:<6}" + "".join([f"{fmt_p(v):>10}" for v in row.values])
        write_line(f, line)

    # 有意ペア（CSVにも）
    pairs = analyzer.significant_pairs(scale=scale, subscale=subscale, alpha_low=0.01, alpha_high=0.05, unique=True)
    def to_df(sig_pairs: list[tuple[str, str]], tag: str):
        if not sig_pairs:
            return pd.DataFrame(columns=["scale", "subscale", "alpha", "winner", "loser"])
        df = pd.DataFrame(sig_pairs, columns=["winner", "loser"])
        df["alpha"] = tag; df["scale"] = scale; df["subscale"] = subscale
        df["winner"] = df["winner"].map(lambda x: ABBR.get(x, x))
        df["loser"]  = df["loser"].map(lambda x: ABBR.get(x, x))
        return df[["scale", "subscale", "alpha", "winner", "loser"]]
    add_table("Inferential", "nemenyi_significant_pairs",
              pd.concat([to_df(pairs.get("0.01", []), "≤.01"),
                         to_df(pairs.get("0.05", []), ".01<≤.05")], ignore_index=True))

    # テキスト：有意ペア
    def fmt_pairs(plist):
        if not plist:
            return "(none)"
        return ", ".join([f"{ABBR.get(a, a)}>{ABBR.get(b, b)}" for (a, b) in plist])
    write_line(f, f"  Significant pairs @ p≤.01: {fmt_pairs(pairs.get('0.01', []))}")
    write_line(f, f"  Significant pairs @ .01<p≤.05: {fmt_pairs(pairs.get('0.05', []))}")

    # 既存の保存（CSV/TeX）も継続
    if save_dir:
        os.makedirs(save_dir, exist_ok=True)
        tag = f"{prefix}{scale.replace('/', '-')}_{subscale}".replace(" ", "")
        ptab_txt.to_csv(os.path.join(save_dir, f"nemenyi_{tag}.csv"), encoding="utf-8")
        with open(os.path.join(save_dir, f"nemenyi_{tag}.tex"), "w", encoding="utf-8") as tf:
            tf.write(ptab_txt.to_latex())

# ========== メイン ==========
def main():
    parser = argparse.ArgumentParser(
        formatter_class=argparse.RawDescriptionHelpFormatter,
        description=textwrap.dedent(__doc__ or "")
    )
    parser.add_argument("--csv",  default=DEFAULT_CSV,   help="スコア集計用CSV (SUS/TAM/TPA/NASA)")
    parser.add_argument("--out",  default=DEFAULT_OUTDIR,help="出力フォルダ")
    parser.add_argument("--pref", default=DEFAULT_PREF,  help="好み順位CSV（任意・渡せばBradley–Terryを追記）")
    # Jupyter等が勝手に付ける -f を含む未知引数を無視
    args, _unknown = parser.parse_known_args()

    CSV_PATH = os.path.abspath(args.csv)
    OUT_DIR  = os.path.abspath(args.out)
    OUT_TXT  = os.path.join(OUT_DIR, "summary.txt")

    os.makedirs(OUT_DIR, exist_ok=True)

    # CSV 読み込み（ヘッダあり/なしフォールバック）
    try:
        df = pd.read_csv(CSV_PATH, header=0)
    except Exception:
        df = pd.read_csv(CSV_PATH, header=None)

    analyzer = SurveyResultAnalyzer(df)

    with open(OUT_TXT, "w", encoding="utf-8") as f:
        # ヘッダ
        title = "User Study Summary (SUS / TAM / TPA / NASA-TLX)"
        write_section_header(f, title)
        write_line(f, f"Generated: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
        write_line(f, f"Source CSV: {os.path.relpath(CSV_PATH)}")
        write_line(f)

        # ========== 記述統計 ==========
        write_section_header(f, "Descriptive Statistics (median / IQR / mean / SD / n / 95%CI)")

        # SUS
        sus_desc, sus_has_ci = safe_describe(analyzer.describe_sus, with_ci=True)
        write_line(f, "[SUS / Total]")
        for line in describe_block_to_lines(sus_desc, want_ci=sus_has_ci):
            write_line(f, line)
        write_line(f)
        add_table("Descriptive", "sus_total", desc_dict_to_df(sus_desc, "SUS", "Total", sus_has_ci))

        # TPA
        tpa_desc, tpa_has_ci = safe_describe(analyzer.describe_tpa, with_ci=True)
        write_line(f, "[TPA / Total]")
        for line in describe_block_to_lines(tpa_desc, want_ci=tpa_has_ci):
            write_line(f, line)
        write_line(f)
        add_table("Descriptive", "tpa_total", desc_dict_to_df(tpa_desc, "TPA", "Total", tpa_has_ci))

        # TAM（PEOU / PU / Total）
        tam_desc, tam_has_ci = safe_describe(analyzer.describe_tam, with_ci=True)
        write_line(f, "[TAM / PEOU, PU, Total]")
        flat = {}
        for cond, subdict in tam_desc.items():
            for sub, s in subdict.items():
                flat.setdefault(sub, {})[cond] = s
        for sub in ("PEOU", "PU", "Total"):
            write_line(f, f"  - {sub}")
            sub_dict = flat.get(sub, {})
            for line in describe_block_to_lines(sub_dict, want_ci=tam_has_ci):
                write_line(f, "    " + line)
            write_line(f)
            add_table("Descriptive", f"tam_{sub.lower()}", desc_dict_to_df(sub_dict, "TAM", sub, tam_has_ci))
        write_line(f)

        # NASA-TLX（各次元 + Overall）
        tlx_desc, tlx_has_ci = safe_describe(analyzer.describe_nasa_tlx, with_ci=True)
        write_line(f, "[NASA-TLX / Mental, Physical, Temporal, Performance(rev), Effort, Frustration, Overall]")
        # Overall
        write_line(f, "  - Overall")
        overall_dict = {cond: stats_dict["Overall"] for cond, stats_dict in tlx_desc.items()}
        for line in describe_block_to_lines(overall_dict, want_ci=tlx_has_ci):
            write_line(f, "    " + line)
        write_line(f)
        add_table("Descriptive", "nasa_overall", desc_dict_to_df(overall_dict, "NASA-TLX", "Overall", tlx_has_ci))

        # 各次元
        for dim in ["Mental", "Physical", "Temporal", "Performance", "Effort", "Frustration"]:
            write_line(f, f"  - {dim}")
            dim_dict = {cond: stats_dict[dim] for cond, stats_dict in tlx_desc.items()}
            for line in describe_block_to_lines(dim_dict, want_ci=tlx_has_ci):
                write_line(f, "    " + line)
            write_line(f)
            add_table("Descriptive", f"nasa_{dim.lower()}", desc_dict_to_df(dim_dict, "NASA-TLX", dim, tlx_has_ci))
        write_line(f)

        # ========== 推測統計 ==========
        write_section_header(f, "Inferential Statistics (Friedman / Kendall's W / Nemenyi)")
        targets = [
            ("SUS", "Total"),
            ("TAM", "Total"),
            ("TPA", "Total"),
            ("NASA-TLX", "Overall"),
        ]
        for scale, sub in targets:
            write_line(f, f"[{scale} / {sub}]")
            friedman_block_with_csv(f, analyzer, scale, sub, save_dir=OUT_DIR, prefix="")
            write_line(f)

        # ========== 信頼性 ==========
        write_section_header(f, "Scale Reliability (Cronbach’s alpha)")
        try:
            alpha = analyzer.reliability_all()
            for k, v in alpha.items():
                write_line(f, f"- {k:9s}: α = {v:.3f}")
            # CSVにも
            alpha_df = pd.DataFrame([{"scale": k, "alpha": v} for k, v in alpha.items()])
            add_table("Reliability", "cronbach_alpha", alpha_df)
        except Exception as e:
            write_line(f, f"(reliability_all でエラー: {e})")
        write_line(f)

        # ========== （任意）好みのBradley–Terry ==========
        if args.pref:
            write_section_header(f, "Preference (Bradley–Terry)")
            bt_text, bt_dfs = run_bradley_terry(args.pref)
            write_line(f, bt_text)
            write_line(f)
            # CSV集約
            add_table("Preference", "bt_scores",       bt_dfs.get("bt_scores"))
            add_table("Preference", "bt_beta",         bt_dfs.get("bt_beta"))
            add_table("Preference", "bt_wins_matrix",  bt_dfs.get("bt_wins_matrix"))
            add_table("Preference", "bt_info",         bt_dfs.get("bt_info"))

        write_line(f, "End of report.")

    # 付帯: 出力ログ
    print("✅ Exported:")
    print(" -", os.path.relpath(OUT_TXT))
    print(" -", os.path.relpath(os.path.join(OUT_DIR, "nemenyi_SUS_Total.csv")))
    print(" -", os.path.relpath(os.path.join(OUT_DIR, "nemenyi_TAM_Total.csv")))
    print(" -", os.path.relpath(os.path.join(OUT_DIR, "nemenyi_TPA_Total.csv")))
    print(" -", os.path.relpath(os.path.join(OUT_DIR, "nemenyi_NASA-TLX_Overall.csv")))

    # 単一CSVを書き出し
    if CSV_TABLES:
        out_csv = pd.concat(CSV_TABLES, ignore_index=True)
        out_path = os.path.join(OUT_DIR, DEFAULT_OUTCSV)
        out_csv.to_csv(out_path, index=False, encoding="utf-8")
        print(" -", os.path.relpath(out_path))

if __name__ == "__main__":
    main()
