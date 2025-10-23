# 高度統計レポート（IMWUT向け）


## T_task_sec


Friedman: χ²=47.5043, p=2.7148e-10, N=23


### 記述統計（参加者×条件メディアン）

Condition | Median | Q1 | Q3 | Mean | SD
---|---:|---:|---:|---:|---:
SR | 63.1 | 56.9 | 85.7 | 76.4 | 35.4
P+SR | 37.3 | 29.2 | 43 | 39 | 14.3
Pointing | 34.5 | 28 | 38 | 35.2 | 9.86
Label | 51.5 | 40.9 | 64.9 | 54.1 | 17.2

### ペアごとの Wilcoxon（Holm 補正）

Pair | n | W | p_raw | p_holm | effect_r
---|---:|---:|---:|---:|---:
SR vs P+SR | 23 | 2 | 7.15e-07 | 3.58e-06 | 0.863
SR vs Pointing | 23 | 0 | 2.38e-07 | 1.43e-06 | 0.875
SR vs Label | 23 | 42 | 0.00242 | 0.00483 | 0.609
P+SR vs Pointing | 23 | 111 | 0.427 | 0.427 | 0.171
P+SR vs Label | 23 | 29 | 0.000408 | 0.00122 | 0.691
Pointing vs Label | 23 | 13 | 2.1e-05 | 8.39e-05 | 0.793

## N_cmds


Friedman: χ²=26.7778, p=6.55392e-06, N=23


### 記述統計（参加者×条件メディアン）

Condition | Median | Q1 | Q3 | Mean | SD
---|---:|---:|---:|---:|---:
SR | 2 | 1.75 | 2 | 2.07 | 0.921
P+SR | 1 | 1 | 1 | 1.15 | 0.463
Pointing | 2 | 2 | 2 | 2.02 | 0.104
Label | 1.5 | 1 | 2 | 1.67 | 0.684

### ペアごとの Wilcoxon（Holm 補正）

Pair | n | W | p_raw | p_holm | effect_r
---|---:|---:|---:|---:|---:
SR vs P+SR | 23 | 22 | 0.00226 | 0.0113 | 0.736
SR vs Pointing | 23 | 33 | 1 | 1 | 0.666
SR vs Label | 23 | 40 | 0.141 | 0.281 | 0.622
P+SR vs Pointing | 23 | 11.5 | 3.94e-05 | 0.000236 | 0.802
P+SR vs Label | 23 | 23 | 0.0188 | 0.0753 | 0.729
Pointing vs Label | 23 | 37 | 0.03 | 0.0901 | 0.641

## N_actuations


Friedman: χ²=21.6651, p=7.65804e-05, N=23


### 記述統計（参加者×条件メディアン）

Condition | Median | Q1 | Q3 | Mean | SD
---|---:|---:|---:|---:|---:
SR | 4 | 3.5 | 5.75 | 4.91 | 2.49
P+SR | 3 | 3 | 3.25 | 3.17 | 1.28
Pointing | 5 | 3.75 | 5.5 | 4.52 | 1.02
Label | 3.5 | 3.5 | 5 | 4.33 | 1.34

### ペアごとの Wilcoxon（Holm 補正）

Pair | n | W | p_raw | p_holm | effect_r
---|---:|---:|---:|---:|---:
SR vs P+SR | 23 | 27.5 | 0.00373 | 0.0149 | 0.701
SR vs Pointing | 23 | 114 | 0.958 | 1 | 0.152
SR vs Label | 23 | 80.5 | 0.556 | 1 | 0.365
P+SR vs Pointing | 23 | 18.5 | 0.00198 | 0.00992 | 0.758
P+SR vs Label | 23 | 20 | 0.00143 | 0.00856 | 0.748
Pointing vs Label | 23 | 104 | 0.453 | 1 | 0.219

## 混合効果モデル：log(T_task_sec)

### MixedLM: log(T_task_sec) ~ condition + devices + colors + condition×devices

Fixed effects (β), 95%CI, and % change (exp(β)-1)*100.

                                         term          beta        ci_low       ci_high    pct_change
                                    Intercept  8.515856e-16 -4.257859e-02  4.257859e-02  8.881784e-14
                         C(condition)[T.P+SR] -2.703332e-01 -3.897805e-01 -1.508860e-01 -2.368749e+01
                     C(condition)[T.Pointing] -3.812140e-01 -5.006174e-01 -2.618106e-01 -3.169683e+01
                           C(condition)[T.SR]  3.323132e-01  2.126856e-01  4.519408e-01  3.941894e+01
                         n_devices_required_c  1.973879e-02 -3.550614e-02  7.498372e-02  1.993489e+00
    n_devices_required_c:C(condition)[T.P+SR]  4.382683e-02 -3.079576e-02  1.184494e-01  4.480141e+00
n_devices_required_c:C(condition)[T.Pointing]  2.555038e-02 -4.636670e-02  9.746746e-02  2.587959e+00
      n_devices_required_c:C(condition)[T.SR]  8.439529e-02  1.758442e-02  1.512062e-01  8.805891e+00
                          n_colors_required_c  3.300753e-01  2.635146e-01  3.966360e-01  3.910729e+01
                                    Group Var  0.000000e+00 -1.719704e+20  1.719704e+20  0.000000e+00



## GEE：N_cmds（カウント；過分散に応じ Poisson/NB）

### GEE: N_cmds ~ condition + devices + colors + condition×devices  (family=Poisson)

Coefficients as IRR=exp(β), 95%CI.

                                         term      beta    ci_low   ci_high      IRR  IRR_low  IRR_high
                                    Intercept  0.690186  0.568433  0.811938 1.994086 1.765498  2.252269
                         C(condition)[T.P+SR] -0.354314 -0.551062 -0.157565 0.701655 0.576337  0.854221
                     C(condition)[T.Pointing] -0.025589 -0.161985  0.110807 0.974736 0.850454  1.117179
                           C(condition)[T.SR]  0.151603  0.014924  0.288283 1.163698 1.015035  1.334135
                         n_devices_required_c  0.039741  0.006073  0.073408 1.040541 1.006091  1.076170
    n_devices_required_c:C(condition)[T.P+SR]  0.011381 -0.036502  0.059264 1.011446 0.964156  1.061056
n_devices_required_c:C(condition)[T.Pointing]  0.026283 -0.016164  0.068729 1.026631 0.983966  1.071146
      n_devices_required_c:C(condition)[T.SR]  0.037653 -0.000450  0.075757 1.038371 0.999550  1.078700
                          n_colors_required_c  0.282063  0.232105  0.332021 1.325862 1.261253  1.393782



### 条件のモデルベース比較（IRR比；Holm補正）

Pair | est | se | z | p_raw | p_holm | IRR_ratio
---|---:|---:|---:|---:|---:|---:
SR vs P+SR | 0.506 | 0.0883 | 5.73 | 9.92e-09 | 5.95e-08 | 1.66
SR vs Pointing | 0.177 | 0.0707 | 2.51 | 0.0122 | 0.0367 | 1.19
SR vs Label | 0.152 | 0.0697 | 2.17 | 0.0297 | 0.0594 | 1.16
P+SR vs Pointing | -0.329 | 0.0657 | -5 | 5.64e-07 | 2.82e-06 | 0.72
P+SR vs Label | -0.354 | 0.1 | -3.53 | 0.000416 | 0.00166 | 0.702
Pointing vs Label | -0.0256 | 0.0696 | -0.368 | 0.713 | 0.713 | 0.975

## 媒介の示唆（N_cmds が logT を媒介するか）

### Mediation (heuristic): effect shrinkage when adding N_cmds
                    term  beta_reduced  beta_full   shrink
    C(condition)[T.P+SR]     -0.270070  -0.131770 0.512091
C(condition)[T.Pointing]     -0.379544  -0.372989 0.017271
      C(condition)[T.SR]      0.338743   0.248977 0.264998


## ロバスト性（上位1%トリムの再推定）

### MixedLM: log(T_task_sec) ~ condition + devices + colors + condition×devices

Fixed effects (β), 95%CI, and % change (exp(β)-1)*100.

                                         term          beta        ci_low       ci_high    pct_change
                                    Intercept  8.700607e-16 -3.459692e-02  3.459692e-02  8.881784e-14
                         C(condition)[T.P+SR] -2.705127e-01 -3.894056e-01 -1.516199e-01 -2.370118e+01
                     C(condition)[T.Pointing] -3.808789e-01 -4.997281e-01 -2.620297e-01 -3.167394e+01
                           C(condition)[T.SR]  3.252160e-01  2.061436e-01  4.442884e-01  3.843296e+01
                         n_devices_required_c  2.012074e-02 -3.486777e-02  7.510925e-02  2.032453e+00
    n_devices_required_c:C(condition)[T.P+SR]  4.322586e-02 -3.105036e-02  1.175021e-01  4.417371e+00
n_devices_required_c:C(condition)[T.Pointing]  2.540233e-02 -4.618095e-02  9.698561e-02  2.572772e+00
      n_devices_required_c:C(condition)[T.SR]  8.362674e-02  1.712598e-02  1.501275e-01  8.722300e+00
                          n_colors_required_c  3.285122e-01  2.622605e-01  3.947640e-01  3.889002e+01
                                    Group Var  0.000000e+00 -1.161773e+20  1.161773e+20  0.000000e+00



## 色×条件 交互作用（logT, MixedLM）

                                         term          beta    ci_low   ci_high    pct_change
                                    Intercept  8.615930e-16 -0.024511  0.024511  8.881784e-14
                         C(condition)[T.P+SR] -2.703088e-01 -0.389645 -0.150973 -2.368562e+01
                     C(condition)[T.Pointing] -3.820761e-01 -0.501339 -0.262813 -3.175569e+01
                           C(condition)[T.SR]  3.307565e-01  0.211263  0.450250  3.920209e+01
                         n_devices_required_c  1.212635e-02 -0.049401  0.073654  1.220017e+00
    n_devices_required_c:C(condition)[T.P+SR]  5.141557e-02 -0.035388  0.138219  5.276030e+00
n_devices_required_c:C(condition)[T.Pointing]  2.158387e-02 -0.060282  0.103449  2.181848e+00
      n_devices_required_c:C(condition)[T.SR]  1.003667e-01  0.026221  0.174512  1.055763e+01
                          n_colors_required_c  3.647672e-01  0.225248  0.504287  4.401787e+01
     n_colors_required_c:C(condition)[T.P+SR] -3.448967e-02 -0.230882  0.161903 -3.390168e+00
 n_colors_required_c:C(condition)[T.Pointing]  2.863106e-02 -0.163841  0.221104  2.904486e+00
       n_colors_required_c:C(condition)[T.SR] -1.191995e-01 -0.306410  0.068011 -1.123693e+01


### 色ごとの P+SR vs Pointing の予測時間比（<1: P+SRが速い）

 colors  est_logT       se        z    p_raw  time_ratio_PSR/Pointing   p_holm
      1  0.165522 0.104035 1.591031 0.111603                 1.180009 0.285034
      2  0.102402 0.061335 1.669536 0.095011                 1.107828 0.285034
      3  0.039281 0.125398 0.313251 0.754090                 1.040063 0.754090


## 被験者内スピアマン相関（センタリング）

- within Spearman ρ(logT_w, dev_w) = 0.423, p=1.18e-50, N=1139
- within Spearman ρ(logT_w, col_w) = 0.437, p=2.6e-54, N=1139


## 変動の確認

- devices unique values: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 19]

- colors unique values:  [1, 2, 3]
