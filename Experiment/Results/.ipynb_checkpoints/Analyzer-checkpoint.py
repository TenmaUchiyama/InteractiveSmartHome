import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
import seaborn as sns
from scipy.stats import kruskal
import scikit_posthocs as sp
import matplotlib

# 日本語フォント設定
matplotlib.rcParams['font.family'] = 'Meiryo'

# --- 定数定義 ---
CATEGORY_COLUMNS = {
    'sus':     list(range(1, 11)),   # SUS 1–10
    'tam':     list(range(11, 14)),  # TAM 11–13
    'trust':   list(range(14, 18)),  # Trust 14–17
    'trust2':  list(range(18, 29)),  # Trust2 18–28
    'nasa':    list(range(29, 38)),  # NASA-TLX 29–37
}
PARTICIPANT_COL = 38
CONDITION_COL   = 39
INVALID_IDS     = ['0', 'TEST', 'P2', 'OLD_P1']
CONDITION_MAP = {
    1: "Pointing+SpatialReference",
    2: "Label",
    3: "Pointing",
    4: "SpatialReference"
}

# --- データ読み込み & 前処理 ---
def load_and_preprocess(filepath: str) -> pd.DataFrame:
    df = pd.read_csv(filepath, encoding='utf-8')
    df['participant_id'] = df.iloc[:, PARTICIPANT_COL].astype(str)
    df['condition']      = df.iloc[:, CONDITION_COL].astype(int)
    df = df[~df['participant_id'].isin(INVALID_IDS)].reset_index(drop=True)
    for key, cols in CATEGORY_COLUMNS.items():
        df[key] = df.iloc[:, cols].apply(lambda r: r.astype(int).tolist(), axis=1)
    return df

# --- スコア計算 ---
def calc_sus_score(raw: list[int]) -> float:
    adj = [(s - 1) if i % 2 == 0 else (5 - s) for i, s in enumerate(raw)]
    return sum(adj) * 2.5

def compute_mean(vals: list[int]) -> float:
    return np.mean(vals)

def reverse_scoring(df: pd.DataFrame) -> pd.DataFrame:
    df['trust2'] = df['trust2'].apply(lambda v: [6 - x if i in (8,9) else x for i, x in enumerate(v)])
    df['nasa']   = df['nasa'].apply(lambda v: [6 - x if i == 3 else x for i, x in enumerate(v)])
    return df

# --- 統計検定用 ---
def run_kruskal(label: str, df: pd.DataFrame) -> tuple[float, float]:
    groups = [df[df['condition'] == c][label] for c in CONDITION_MAP.values()]
    return kruskal(*groups)

# --- メイン ---
def main():
    # データ読み込み・前処理
    df = load_and_preprocess('SurvayResult.csv')
    df = reverse_scoring(df)

    # スコア DataFrame 作成
    metrics = {
        'SUS':     ('sus',    calc_sus_score, 0, 100),
        'TAM':     ('tam',    compute_mean,   1,   7),
        'Trust':   ('trust',  compute_mean,   1,   7),
        'Trust2':  ('trust2', compute_mean,   1,   7),
        'NASA-TLX':('nasa',   compute_mean,   1,   7),
    }
    dfs = {}
    for name, (key, func, ymin, ymax) in metrics.items():
        dfs[name] = pd.DataFrame({
            'participant_id': df['participant_id'],
            'condition':      df['condition'],
            name:              df[key].apply(func)
        })

    # 条件ラベル適用
    for dfm in dfs.values():
        dfm['condition'] = dfm['condition'].map(CONDITION_MAP)

    # 1. 基本プロット: 箱ひげ＋スウォーム & 平均±SEバー
    fig1, axes1 = plt.subplots(len(metrics), 2, figsize=(14, 5*len(metrics)))
    for i, (name, dfm) in enumerate(dfs.items()):
        ymin, ymax = metrics[name][2], metrics[name][3]
        # 箱ひげ＋スウォーム
        ax = axes1[i, 0]
        sns.boxplot(data=dfm, x='condition', y=name, palette='pastel', ax=ax)
        sns.swarmplot(data=dfm, x='condition', y=name, color='black', alpha=0.7, ax=ax)
        ax.set(title=f"{name} の条件別比較", ylim=(ymin, ymax))
        # 平均±SEバー
        ax2 = axes1[i, 1]
        means = dfm.groupby('condition')[name].mean()
        sems  = dfm.groupby('condition')[name].sem()
        ax2.bar(means.index.astype(str), means, yerr=sems, capsize=5, color='skyblue')
        ax2.set(title=f"{name} 平均 ± SE", ylim=(ymin, ymax))
    plt.tight_layout(); plt.suptitle('条件別ユーザビリティ・受容性評価', y=1.02)
    plt.savefig('survey_plots.png', dpi=300)


    stats = {}
    for name in ['SUS','TAM','Trust','Trust2']:
        print(f"\n🔎 {name} の有意差検定")
        stat, p = run_kruskal(name, dfs[name])
        stats[name] = (stat, p)
        print(f"{name}: H={stat:.3f}, p={p:.3f}")
        

    # 3. 統計結果付きボックスプロット
    fig2, axes2 = plt.subplots(2, 2, figsize=(12, 10))
    for ax, name in zip(axes2.flat, ['SUS','TAM','Trust','Trust2']):
        dfm = dfs[name]
        stat, p = stats[name]
        sns.boxplot(data=dfm, x='condition', y=name, palette='pastel', ax=ax)
        ax.set_title(f"{name} (H={stat:.2f}, p={p:.3f})")
        ax.set_xlabel(''); ax.set_ylabel(name)
    plt.tight_layout(); plt.suptitle('統計検定結果付き分布図', y=1.02)
    plt.savefig('stats_boxplots.png', dpi=300)
    plt.show()


    for name in ['SUS','TAM','Trust','Trust2']:
        stat, p = stats[name]
        dfm = dfs[name]
        if p < 0.05:
            print(f"\n➡️ {name} に有意差あり → Dunn’s test（Bonferroni補正）")
            posthoc = sp.posthoc_dunn(dfm, val_col=name, group_col="condition", p_adjust='bonferroni')
            print(posthoc)
        else:
            print(f"\n❎ {name} に有意差なし → Dunn’s test はスキップ")


if __name__ == '__main__':
    main()  
