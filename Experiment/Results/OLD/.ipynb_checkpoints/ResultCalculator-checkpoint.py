from scipy.stats import friedmanchisquare
import scikit_posthocs as sp
import pandas as pd
import numpy as np

class SurveyResultAnalyzer:
    """
    前提:
      - DataFrameは「1人=4行」(各行が条件)
      - 条件は列index=2 ('Condition') に 1..4 で入っている
      - 下記インデックス範囲にSUS/TAM/TPA/NASA-TLXの各項目がある
    目的:
      - 条件ごと (SpatialReference / SP+Pointing / Pointing / Label) に
        SUS / TAM(PEOU,PU,Total) / TPA / NASA-TLX(6次元+総合) を集計（mean, sd, n）
    """
    # 列インデックスの分類（0始まり）
    COLS = {
        "SUS": list(range(3, 13)),        # 10項目 (Likert 1-5想定)
        "TAM": list(range(13, 25)),       # 12項目 (1-7)
        "TPA": list(range(25, 37)),       # 12項目 (1-7; 前半5がネガティブ)
        "NASA_TLX": list(range(37, 43)),  # 6項目 (1-21; Performanceは反転)
    }

    # 条件ID -> ラベル
    CONDITION_MAP = {
        1: "SpatialReference",
        2: "SpatialReference+Pointing",
        3: "Pointing",
        4: "Label",
    }

    def __init__(self, df: pd.DataFrame):
        self.df = df.reset_index(drop=True)

        # 行ごとに: Condition と各スケール回答(数値化)を保持
        self.rows = []
        for i in range(len(self.df)):
            row = self.df.iloc[i]
            cond_id = int(row.iloc[2])  # Conditionは列index=2
            self.rows.append({
                "ConditionId": cond_id,
                "Condition": self.CONDITION_MAP.get(cond_id, f"Cond{cond_id}"),
                "SUS":  row.iloc[self.COLS["SUS"]].astype(float).to_list(),
                "TAM":  row.iloc[self.COLS["TAM"]].astype(float).to_list(),
                "TPA":  row.iloc[self.COLS["TPA"]].astype(float).to_list(),
                "NASA_TLX": row.iloc[self.COLS["NASA_TLX"]].astype(float).to_list(),
            })

    # ========= 単一行(=条件×被験者)のスコア計算 =========

    @staticmethod
    def _calc_sus_from_responses(responses):
        # 10項目: 奇数(1,3,5,7,9)は r-1, 偶数は 5-r, 合計×2.5 → 0-100
        scores = [(r - 1 if (i % 2 == 1) else 5 - r) for i, r in enumerate(responses, start=1)]
        return sum(scores) * 2.5

    @staticmethod
    def _calc_tam_from_responses(responses):
        # 12項目: PEOU(前6)とPU(後6)の平均、Totalは全体平均
        peou = float(np.mean(responses[:6]))
        pu   = float(np.mean(responses[6:]))
        total = float(np.mean(responses))
        return {"PEOU": peou, "PU": pu, "Total": total}

    
    @staticmethod
    def _calc_tpa_from_responses(responses):
        # 1..7尺度想定
        # 項目順:
        # [confident, security, integrity, dependable, reliable, trust, familiar,  
        #  deceptive, underhanded, suspicious, wary, harmful]                   
        pos = responses[:7]
        neg = [8 - r for r in responses[7:]]  # 後半5を反転
        all_scores = pos + neg
        return float(np.mean(all_scores))

    @staticmethod
    def _calc_nasatlx_from_responses(responses, scale_max=21):
        # 項目順: [Mental, Physical, Temporal, Performance, Effort, Frustration]
        # Performanceは反転 (scale_max - r + 1 ではなく、ここでは 1..scale_max想定 → scale_max+1-r)
        perf_rev = (scale_max + 1) - responses[3]
        dims = {
            "Mental":       responses[0],
            "Physical":     responses[1],
            "Temporal":     responses[2],
            "Performance":  perf_rev,    # 反転済み
            "Effort":       responses[4],
            "Frustration":  responses[5],
        }
        overall = float(np.mean(list(dims.values())))
        return {"dims": dims, "Overall": overall}

    # ========= 条件ごとの記述統計 (mean, sd, n) =========

    @staticmethod
    def _describe(values):
        values = list(values)
        n = len(values)
        if n == 0:
            return {"mean": np.nan, "std": np.nan, "median": np.nan, "n": 0}
        return {
            "mean": float(np.mean(values)),
            "std":  float(np.std(values, ddof=1)) if n > 1 else 0.0,
            "median": float(np.median(values)),
            "n":    n,
        }

    def describe_sus(self):
        """条件ごとの SUS (0-100) の mean, sd, n を返す"""
        out = {}
        for cond_id, cond_name in self.CONDITION_MAP.items():
            scores = [
                self._calc_sus_from_responses(r["SUS"])
                for r in self.rows if r["ConditionId"] == cond_id
            ]
            out[cond_name] = self._describe(scores)
        return out

    def describe_tpa(self):
        """条件ごとの TPA (反転込み平均) の mean, sd, n"""
        out = {}
        for cond_id, cond_name in self.CONDITION_MAP.items():
            scores = [
                self._calc_tpa_from_responses(r["TPA"])
                for r in self.rows if r["ConditionId"] == cond_id
            ]
            out[cond_name] = self._describe(scores)
        return out

    def describe_tam(self):
        """
        条件ごとに PEOU / PU / Total の mean, sd, n を返す:
        {
          cond: {
            "PEOU": {...}, "PU": {...}, "Total": {...}
          }, ...
        }
        """
        out = {}
        for cond_id, cond_name in self.CONDITION_MAP.items():
            peou_vals, pu_vals, total_vals = [], [], []
            for r in self.rows:
                if r["ConditionId"] != cond_id:
                    continue
                s = self._calc_tam_from_responses(r["TAM"])
                peou_vals.append(s["PEOU"])
                pu_vals.append(s["PU"])
                total_vals.append(s["Total"])
            out[cond_name] = {
                "PEOU":  self._describe(peou_vals),
                "PU":    self._describe(pu_vals),
                "Total": self._describe(total_vals),
            }
        return out

    def describe_nasa_tlx(self, scale_max=21):
        """
        条件ごとに NASA-TLX の各次元 + Overall の mean, sd, n を返す:
        {
          cond: {
            "Mental": {...}, "Physical": {...}, ..., "Overall": {...}
          }, ...
        }
        """
        out = {}
        for cond_id, cond_name in self.CONDITION_MAP.items():
            dim_vals = {k: [] for k in ["Mental","Physical","Temporal","Performance","Effort","Frustration"]}
            overall_vals = []
            for r in self.rows:
                if r["ConditionId"] != cond_id:
                    continue
                res = self._calc_nasatlx_from_responses(r["NASA_TLX"], scale_max=scale_max)
                for k, v in res["dims"].items():
                    dim_vals[k].append(v)
                overall_vals.append(res["Overall"])

            out[cond_name] = {k: self._describe(vs) for k, vs in dim_vals.items()}
            out[cond_name]["Overall"] = self._describe(overall_vals)
        return out

    # ========= 可視化/集計用のロング形式を出す補助 =========

    def to_long_scores(self):
        """
        箱ひげ図などにすぐ使える長い形式のDataFrameを返す。
        列: ['Condition', 'Scale', 'Subscale', 'Score']
        SUS/TPA: Subscale='Total'
        TAM: Subscale in {'PEOU','PU','Total'}
        NASA-TLX: Subscale in {'Mental','Physical','Temporal','Performance','Effort','Frustration','Overall'}
        """
        records = []

        for r in self.rows:
            cond = r["Condition"]

            # SUS
            sus = self._calc_sus_from_responses(r["SUS"])
            records.append({"Condition": cond, "Scale": "SUS", "Subscale": "Total", "Score": sus})

            # TPA
            tpa = self._calc_tpa_from_responses(r["TPA"])
            records.append({"Condition": cond, "Scale": "TPA", "Subscale": "Total", "Score": tpa})

            # TAM
            tam = self._calc_tam_from_responses(r["TAM"])
            for sub in ["PEOU", "PU", "Total"]:
                records.append({"Condition": cond, "Scale": "TAM", "Subscale": sub, "Score": tam[sub]})

            # NASA-TLX
            nas = self._calc_nasatlx_from_responses(r["NASA_TLX"])
            for sub, v in nas["dims"].items():
                records.append({"Condition": cond, "Scale": "NASA-TLX", "Subscale": sub, "Score": v})
            records.append({"Condition": cond, "Scale": "NASA-TLX", "Subscale": "Overall", "Score": nas["Overall"]})

        return pd.DataFrame.from_records(records)
    
    
    # === Friedman & Nemenyi のユーティリティ ===
    def friedman_nemenyi(self, scale="SUS", subscale="Total"):
        """
        任意スケールのFriedman検定＋Nemenyi事後検定
        """
        # 1) ロング→対象のみ抽出（順序は元DFの並びを維持）
        long_df = self.to_long_scores()
        df = long_df[(long_df["Scale"] == scale) & (long_df["Subscale"] == subscale)].reset_index(drop=True)

        # 2) 早期ガード
        if df.empty:
            raise ValueError(f"No data for scale={scale}, subscale={subscale}.")
        # 「4行=1人」を前提にIDを付与（並びが崩れているとここで気づける）
        if len(df) % 4 != 0:
            raise ValueError(f"Row count ({len(df)}) is not a multiple of 4. Check row ordering per participant.")
        df["Subject"] = df.index // 4

        # 3) ピボット（行=被験者, 列=条件）
        #    列順は CONDITION_MAP の定義順に固定
        cond_cols = [self.CONDITION_MAP[k] for k in sorted(self.CONDITION_MAP.keys())]
        df_wide = (df.pivot(index="Subject", columns="Condition", values="Score")
                     .reindex(columns=cond_cols))

        # 4) 条件が1つでも欠損している被験者は除外
        df_wide = df_wide.dropna()
        if df_wide.empty or (df_wide.shape[1] != 4):
            raise ValueError("Not enough complete blocks for Friedman (need all 4 conditions per subject).")

        # 5) Friedman検定（列順を固定）
        arrays_for_friedman = [df_wide[c].values for c in cond_cols]
        stat, p = friedmanchisquare(*arrays_for_friedman)

        # 6) Nemenyi事後検定（列順を固定して渡す）
        nemenyi_pvals = sp.posthoc_nemenyi_friedman(df_wide[cond_cols])

        return {"friedman": (float(stat), float(p)), "nemenyi": nemenyi_pvals}
        
        
        
    def significant_pairs(self, scale="SUS", subscale="Total",
                           alpha_low=0.01, alpha_high=0.05,
                           unique=True):
        """
        Nemenyiのp値マトリクスから有意ペアを返す。
        返り値:
          {
            "0.01": [(i,j), ...],   # p <= alpha_low
            "0.05": [(i,j), ...],   # alpha_low < p <= alpha_high
          }
        ※ unique=True のとき (i,j) は i<j（順序なしのユニークなペア）
        """
        import numpy as np
        res = self.friedman_nemenyi(scale=scale, subscale=subscale)
        pmat = res["nemenyi"].copy()

        # 条件順・ラベル調整
        cond_cols = [self.CONDITION_MAP[k] for k in sorted(self.CONDITION_MAP.keys())]
        n = len(cond_cols)
        pmat = pmat.iloc[:n, :n]
        pmat.index = cond_cols
        pmat.columns = cond_cols

        name_to_id = {v: k for k, v in self.CONDITION_MAP.items()}

        out = {"0.01": [], "0.05": []}

        # === ここがポイント：ユニークなペアは i<j のみ ===
        if unique:
            idx_pairs = [(i, j) for i in range(n) for j in range(i + 1, n)]
        else:
            idx_pairs = [(i, j) for i in range(n) for j in range(n) if i != j]

        for i, j in idx_pairs:
            p = float(pmat.iat[i, j])
            if not np.isfinite(p):
                continue

            a_id = name_to_id[cond_cols[i]]
            b_id = name_to_id[cond_cols[j]]

            # unique=True の場合は (小さいID, 大きいID) に正規化（保険）
            if unique and a_id > b_id:
                a_id, b_id = b_id, a_id

            if p <= alpha_low:
                out["0.01"].append((a_id, b_id))
            elif p <= alpha_high:
                out["0.05"].append((a_id, b_id))

        return out




    
