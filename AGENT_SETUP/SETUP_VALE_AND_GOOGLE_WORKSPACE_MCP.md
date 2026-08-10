# Vale MCP / Google Workspace MCP セットアップ手順書

対象: Claude Code (Windows) に、Vale(文体・用語チェック)と Google Docs/Drive の読み書きMCPを導入する。
別PCで同じ環境を再現するエージェント向けの手順。上から順に実行すること。

前提: Windows, winget, npm/npx (Node.js), Python が利用可能であること。

---

## 1. Vale MCP(完了・動作確認済み)

### 1-1. Vale本体をインストール

```bash
winget install errata-ai.Vale --accept-package-agreements --accept-source-agreements
```

確認:
```bash
vale --version
```

### 1-2. Claude CodeにMCPとして登録

```bash
claude mcp add vale-mcp -- npx -y @iflow-mcp/chrischinchilla-vale-mcp
```

### 1-3. 動作確認

登録後、Claude Codeのセッションを**再起動**すること(新規追加したMCPのツールはセッション再起動しないと認識されない)。

再起動後、`mcp__vale-mcp__vale_status` ツールが呼び出せれば成功。

---

## 2. Google Workspace MCP(⚠️ 未解決の既知バグあり — 下記「既知の問題」を先に読むこと)

Docs/Driveの読み書きをAIエージェントから行うためのMCP。`taylorwilsdon/google_workspace_mcp` (GitHub 2,993 stars, MITライセンス, `uvx` で実行)を採用。

`mercurial.google-workspace-mcp` というVS Code拡張(publisher: Renzo Johnson)は**採用しないこと**。GitHub上のバックエンドリポジトリがスター0・実質メンテナンスなしで、OAuthスコープの記載とツール機能が矛盾しているなど信頼性が低いと判断した。

### 2-1. uv/uvxのインストール

```bash
winget install --id astral-sh.uv -e --accept-package-agreements --accept-source-agreements
```

確認:
```bash
uv --version
uvx --version
```

### 2-2. Google Cloud Console側の設定(人間の作業が必須)

このステップはブラウザでの手動作業が必要。エージェントは代行できない。以下を**ユーザーに依頼**すること。

1. https://console.cloud.google.com/ を開く
   - ⚠️ 対象のGoogle Docsが大学(Google Workspace組織)アカウント所有の場合、**その組織アカウントでは「親リソース(組織)」の選択に制限がかかっていてプロジェクトを作成できないことが多い**。その場合は**個人のGmailアカウント**でログインしてプロジェクトを作ること(OAuthクライアントの「入れ物」プロジェクトと、実際にログインしてDocsへのアクセスを許可する「認証アカウント」は別物なので、これで問題ない)。
2. 新規プロジェクト作成
3. **APIとサービス → ライブラリ** で以下を有効化:
   - Google Docs API
   - Google Drive API
4. **Google Auth Platform**(旧「OAuth同意画面」。2024年に名称変更された)を設定:
   - メニュー: ☰ → APIとサービス → Google Auth Platform
   - ⚠️ 何かAPIを有効化する前はこのメニュー自体が表示されないことがある。手順3を先に行うこと。
   - **Branding**: 開発者の連絡先情報 = プロジェクトを作成したアカウント(個人アカウントを使った場合はそちら)のメールアドレス
   - **Audience**: User Type = **External** を選択。Test usersに、実際にDocsへアクセスする対象アカウント(大学アカウント等)を追加。
     - ⚠️ Externalかつ「Testing」ステータスのままだと、**リフレッシュトークンの有効期限が7日間**に制限される。恒久利用には別途Google検証(verification)申請が必要だが、個人開発では手間が大きいため、基本は「週1回の再認証」を許容する運用を前提とする。
   - **Data Access**: 「Add or Remove Scopes」から、**明示的に**以下のスコープを追加する。
     - ⚠️ ここが最重要の落とし穴。「APIを有効化しただけ」ではスコープは使えず、`invalid_scope` エラー(Error 400)になる。
     ```
     https://www.googleapis.com/auth/documents
     https://www.googleapis.com/auth/documents.readonly
     https://www.googleapis.com/auth/drive
     https://www.googleapis.com/auth/drive.file
     https://www.googleapis.com/auth/drive.readonly
     ```
   - **Clients**: 「+ CREATE CLIENT」→ Application type = **Desktop app**(Web applicationは選ばないこと。PKCEの扱いが異なり動作しない)。作成後に表示される **Client ID** と **Client Secret** を控える。

### 2-3. MCPサーバーの起動と登録

**重要: stdio transportは使わないこと。** Claude Codeがプロセスを二重起動してしまうバグがあり(下記「既知の問題1」参照)、必ずOAuth認証が失敗する。**streamable-http transportで、Claude Codeの管理下にない独立プロセスとして起動する**こと。

```powershell
$logPath = "C:\Users\<username>\AppData\Local\Temp\workspace-mcp.log"
$uvxPath = "C:\Users\<username>\AppData\Local\Microsoft\WinGet\Packages\astral-sh.uv_Microsoft.Winget.Source_8wekyb3d8bbwe\uvx.exe"
$argList = "/c set GOOGLE_OAUTH_CLIENT_ID=<CLIENT_ID>&& set GOOGLE_OAUTH_CLIENT_SECRET=<CLIENT_SECRET>&& `"$uvxPath`" workspace-mcp --tools docs drive --single-user --transport streamable-http > `"$logPath`" 2>&1"
Start-Process -FilePath "cmd.exe" -ArgumentList $argList -WindowStyle Hidden
```

`Get-NetTCPConnection -LocalPort 8000` でポート8000を1プロセスだけが握っていることを確認してから:

```bash
claude mcp add --transport http google-workspace http://localhost:8000/mcp
```

登録後、Claude Codeのセッションを**再起動**すること。

権限は必要最小限(`--tools docs drive`)に絞ってあり、Gmail/Calendarは含めない。

### 2-4. 初回認証

`mcp__google-workspace__search_docs` 等、任意のツールを呼ぶと認証URLがエラーメッセージとして返る。ユーザーにそのURLをブラウザで開いてもらい、対象のGoogleアカウント(大学アカウント等、Docsの所有者)でログイン・許可してもらう。

---

## 既知の問題(2026-08-10時点で未解決)

`taylorwilsdon/google_workspace_mcp` は本セットアップ中に2種類の致命的バグに遭遇した。別PCでも再現する可能性が高い。

### 既知の問題1: stdio transportでの二重プロセス起動 → PKCE不一致

`claude mcp add google-workspace -- uvx workspace-mcp ...`(stdio方式)で登録すると、Claude Codeが**同一コマンドを同時に2プロセス起動**する(Claude Desktopでも報告されている既知パターン、GitHub Issue #703)。ポート8000のOAuthコールバックサーバーは片方のプロセスにしか立たないため、認証状態(code_verifier)がプロセス間で共有されず、必ず以下のエラーで失敗する:

```
Authentication Processing Error: (invalid_grant) code_verifier or verifier is not needed.
```

**対策: 2-3節の通り、stdioではなくstreamable-http transportで、独立プロセスとして起動すること。**

### 既知の問題2: streamable-http transportでの `ListToolsRequest` ハング

問題1を回避してhttp transportに切り替えても、`claude mcp list` では `✔ Connected` と表示されるにもかかわらず、実際のツール一覧取得(`tools/list`)がサーバー側で永久にハングし、Claude Code側にツールが一切表示されない不具合を確認した。

デバッグログ(`~/.google_workspace_mcp/logs/mcp_server_debug.log`)で確認すると:

```
[server._handle_request:733] - Processing request of type ListToolsRequest
```

の行の後、ログが完全に停止し、応答が返らない。`--single-user` フラグ使用時、認証情報が未確定な状態で `tools/list` を処理しようとして固まっている可能性がある(GitHub Issue #911「OAuthトークンリフレッシュがタイムアウトなしでハングしうる」と類似の症状)。

**この問題は2026-08-10時点で未解決。次にこの作業を再開する際は以下を試すこと:**

1. `--single-user` フラグを外して同様に起動できるか試す
2. `--tools docs drive` の指定を外し、デフォルト(全ツール)で `tools/list` が通るか試す
3. `~/.google_workspace_mcp/credentials/oauth_states.json` に古い期限切れの認証状態が残っていないか確認し、あれば削除してから再試行する
4. それでも解決しない場合は、別のGoogle Docs書き込み対応MCP実装(例: `pm990320/google-workspace-mcp` 等)への切り替えを検討する。ただしスター数・メンテナンス状況を`gh api repos/<owner>/<repo>`で必ず事前確認すること(本セットアップ時に採用基準としてstar数・最終更新日・OAuthスコープ記載とツール機能の整合性を確認した)

### トラブルシューティング用コマンド集

```bash
# ポート8000を握っているプロセスを確認
netstat -ano | grep ":8000"

# そのプロセスの詳細・コマンドラインを確認(二重起動の検知に使う)
powershell -Command "Get-CimInstance Win32_Process -Filter \"Name='python.exe'\" | Select-Object ProcessId,CreationDate,CommandLine"

# workspace-mcpのデバッグログを確認
tail -c 5000 ~/.google_workspace_mcp/logs/mcp_server_debug.log

# 保存済みのOAuth関連ファイルを確認
ls -la ~/.google_workspace_mcp/credentials/
```

---

## 現在の到達状況まとめ(2026-08-10)

| 項目 | 状態 |
|---|---|
| Vale本体 + vale-mcp | ✅ 完了・動作確認済み |
| uv/uvx | ✅ インストール済み |
| Google Cloud Console OAuthクライアント | ✅ 作成済み(Desktop app, Docs/Drive スコープ設定済み, テストユーザー登録済み) |
| google-workspace MCP接続 | ⚠️ `claude mcp list`ではConnectedだが、`tools/list`がハングしツールが使えない(既知の問題2、未解決) |
| PUCドキュメントへの実際の読み書き | ❌ 未達成 |

代替手段として、読み取り専用なら既存の `claude.ai Google Drive`(claude.ai公式コネクタ、Anthropicアカウント連携、追加セットアップ不要)がそのまま使える。書き込みが必要な場合のみ上記の既知の問題を解決する必要がある。
