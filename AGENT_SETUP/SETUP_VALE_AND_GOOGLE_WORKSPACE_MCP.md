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

## 2. Google Workspace MCP(✅ 2026-08-10 に解決・動作確認済み)

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

⚠️ **`cmd /c set VAR=...&&` 方式は使わないこと。** 旧版の手順書はこの方式だったが、2つの問題がある:
1. `set VAR=value&&` は `&&` の直前の空白まで値に含めてしまうため、Client ID/Secretの末尾に空白が混入しうる。この状態でもサーバーは起動し `initialize` にも応答するが、認証情報に触れる最初の呼び出し(`tools/list`)で停止する。**旧「既知の問題2」の原因はこれだった可能性が高い。**
2. 秘密情報がプロセスのコマンドラインに乗るため、`Get-CimInstance Win32_Process` で誰でも読めてしまう。

代わりに、PowerShellの環境変数を子プロセスへ継承させる方式を使う:

```powershell
$env:GOOGLE_OAUTH_CLIENT_ID = "<CLIENT_ID>"
$env:GOOGLE_OAUTH_CLIENT_SECRET = "<CLIENT_SECRET>"
$log = "$env:TEMP\workspace-mcp.log"
$err = "$env:TEMP\workspace-mcp.err.log"
Start-Process -FilePath "$env:USERPROFILE\.local\bin\uvx.exe" `
  -ArgumentList "workspace-mcp --tools docs drive --single-user --transport streamable-http" `
  -RedirectStandardOutput $log -RedirectStandardError $err -WindowStyle Hidden
```

⚠️ `uvx.exe` のパスは環境によって異なる。`(Get-Command uvx).Source` で必ず実際のパスを確認すること(wingetのPackagesディレクトリではなく `~\.local\bin\uvx.exe` にあることが多い)。

⚠️ このプロセスはOSの再起動で消える。**PCを再起動したら毎回この起動コマンドを実行し直す必要がある**(Claude Codeは自動では起動してくれない)。

`Get-NetTCPConnection -LocalPort 8000 -State Listen` でポート8000を1プロセスだけが握っていることを確認してから:

```bash
claude mcp add --transport http google-workspace http://127.0.0.1:8000/mcp
```

⚠️ URLは `localhost` ではなく **`127.0.0.1`** を使うこと。サーバーはIPv4のみにバインドするが、Windowsの `localhost` は `::1`(IPv6)に解決されることがあり、接続できない場合がある。

登録後、Claude Codeのセッションを**再起動**すること。

権限は必要最小限(`--tools docs drive`)に絞ってあり、Gmail/Calendarは含めない。

### 2-4. 初回認証

`mcp__google-workspace__search_docs` 等、任意のツールを呼ぶと認証URLがエラーメッセージとして返る。ユーザーにそのURLをブラウザで開いてもらい、対象のGoogleアカウント(Docsの所有者)でログイン・許可してもらう。

Claude Codeのセッション再起動を待たずに認証だけ先に済ませたい場合は、curlで直接 `start_google_auth` を呼べる(手順は「トラブルシューティング用コマンド集」のMCP直接呼び出しを参照)。

⚠️ `start_google_auth` は `user_google_email` が**必須**。スキーマ上は `"default": null` だが、省略すると `user_google_email must be provided.` で失敗する。

⚠️ 指定するメールアドレスは、2-2で **Test users に登録したアカウントと一致していなければならない**。一致しないと同意画面で `access_denied` になる。

⚠️ 同意画面で「このアプリは確認されていません」と警告が出るが、External + Testing ステータスでは正常。**詳細 → (アプリ名)に移動** で進む。

認証が成功すると `~/.google_workspace_mcp/credentials/<email>.json` が生成され、サーバーログに `Successfully exchanged authorization code for tokens.` が出る。

⚠️ **認証後、生成された認証情報ファイル名を必ず確認すること。** `start_google_auth` に渡したアドレスと、Googleが返す実際のアカウントアドレスは**一致しないことがある**。本セットアップでは `tenma-uchiyama@keio.jp`(ハイフン)で認証URLを生成したが、実際のアカウントは `tenma_uchiyama@keio.jp`(**アンダースコア**)だった。認証自体は成功する(Googleは正しいアカウントで認証する)が、その後のツール呼び出しで**間違ったアドレスを渡すと認証情報が見つからず、再認証を促すエラーが返る**だけで、原因が非常に分かりにくい。

```bash
# 認証済みアカウントの正確な綴りを確認する
ls ~/.google_workspace_mcp/credentials/
```

以降のすべてのツール呼び出しでは、**このファイル名の綴り**を `user_google_email` に使うこと。

複数アカウントを認証した場合、認証情報はアカウントごとに併存する(片方を認証しても他方は消えない)。

---

## 既知の問題(2026-08-10 に両方とも解決済み)

`taylorwilsdon/google_workspace_mcp` は本セットアップ中に2種類の致命的な症状に遭遇した。いずれも回避・解決済みだが、別PCで同じ症状が出たとき用に記録を残す。

### 既知の問題1: stdio transportでの二重プロセス起動 → PKCE不一致

`claude mcp add google-workspace -- uvx workspace-mcp ...`(stdio方式)で登録すると、Claude Codeが**同一コマンドを同時に2プロセス起動**する(Claude Desktopでも報告されている既知パターン、GitHub Issue #703)。ポート8000のOAuthコールバックサーバーは片方のプロセスにしか立たないため、認証状態(code_verifier)がプロセス間で共有されず、必ず以下のエラーで失敗する:

```
Authentication Processing Error: (invalid_grant) code_verifier or verifier is not needed.
```

**対策: 2-3節の通り、stdioではなくstreamable-http transportで、独立プロセスとして起動すること。**

### 既知の問題2: streamable-http transportでの `ListToolsRequest` ハング(解決済み)

問題1を回避してhttp transportに切り替えても、`claude mcp list` では `✔ Connected` と表示されるにもかかわらず、`tools/list` がサーバー側で永久にハングし、Claude Code側にツールが一切表示されない症状が出た。デバッグログ(`~/.google_workspace_mcp/logs/mcp_server_debug.log`)は

```
[server._handle_request:733] - Processing request of type ListToolsRequest
```

の行で完全に停止していた。

**解決:** 2-3節の環境変数継承方式で起動し直したところ、`--single-user` も `--tools docs drive` も付けたまま `tools/list` が**約1秒で37ツールを返した**(サーバー v3.4.6)。当初の対策案(`--single-user` を外す / `--tools` を外す / `oauth_states.json` を消す)はいずれも不要だった。

**原因(推定):** 旧手順の `cmd /c set GOOGLE_OAUTH_CLIENT_ID=<ID>&& ...` は `&&` 直前の空白を値に含めてしまうため、Client ID/Secretの末尾に空白が混入していたと考えられる。この状態でもサーバーは起動し `initialize` には正常応答するため「Connectedなのにツールが出ない」という紛らわしい症状になる。確定はできていないが、起動方法を変えた以外に差分はない。

同じ症状が再発した場合は、まず**認証情報の値が正しいか(前後に空白が入っていないか)**を疑うこと。切り分けには、Claude Codeを介さずcurlで直接 `initialize` → `tools/list` を投げるのが有効(下記コマンド集を参照)。`initialize` は通るのに `tools/list` で止まるなら認証情報を疑う。

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

#### Claude Codeを介さずMCPを直接叩く(切り分け用)

Claude Code側の問題かサーバー側の問題かを切り分けられる。セッション再起動を待たずに認証を先に済ませたいときにも使う。

⚠️ PowerShellから `curl.exe -d '{...}'` とJSONを直接渡すとクォートが壊れて `Parse error` になる。**JSONは必ずファイルに書いて `-d @file.json` で渡すこと。**

```bash
# 1) initialize してセッションIDを取得
printf '{"jsonrpc":"2.0","id":1,"method":"initialize","params":{"protocolVersion":"2024-11-05","capabilities":{},"clientInfo":{"name":"probe","version":"1.0.0"}}}' > init.json
curl.exe -s --max-time 20 -D hdr.txt -o /dev/null -X POST http://127.0.0.1:8000/mcp \
  -H "Content-Type: application/json" -H "Accept: application/json, text/event-stream" -d @init.json
SID=$(grep -i '^mcp-session-id:' hdr.txt | tr -d '\r' | awk '{print $2}')

# 2) initialized 通知(これを送らないと後続が受け付けられない。202が返れば正常)
printf '{"jsonrpc":"2.0","method":"notifications/initialized"}' > ini.json
curl.exe -s --max-time 15 -X POST http://127.0.0.1:8000/mcp \
  -H "Content-Type: application/json" -H "Accept: application/json, text/event-stream" \
  -H "mcp-session-id: $SID" -d @ini.json

# 3) tools/list(ここでハングするか否かが切り分けの要)
printf '{"jsonrpc":"2.0","id":2,"method":"tools/list","params":{}}' > tl.json
curl.exe -s --max-time 60 -X POST http://127.0.0.1:8000/mcp \
  -H "Content-Type: application/json" -H "Accept: application/json, text/event-stream" \
  -H "mcp-session-id: $SID" -d @tl.json > tools_out.txt

# 4) 任意のツールを呼ぶ(例: 認証開始)
printf '{"jsonrpc":"2.0","id":3,"method":"tools/call","params":{"name":"start_google_auth","arguments":{"service_name":"docs","user_google_email":"<EMAIL>"}}}' > call.json
curl.exe -s --max-time 90 -X POST http://127.0.0.1:8000/mcp \
  -H "Content-Type: application/json" -H "Accept: application/json, text/event-stream" \
  -H "mcp-session-id: $SID" -d @call.json
```

応答はSSE形式なので、本文は `grep '^data: ' | sed 's/^data: //'` で取り出してからJSONとしてパースする。応答が大きいと複数の `data:` 行に分割されることがあり、その場合は素朴に連結するとJSONパースに失敗する(`detailed=true` の検索結果などで発生)。切り分け用途では `detailed=false` 等で応答を小さくすると扱いやすい。

---

## 現在の到達状況まとめ(2026-08-10)

| 項目 | 状態 |
|---|---|
| Vale本体 + vale-mcp | ✅ 完了・動作確認済み |
| uv/uvx | ✅ インストール済み(`~\.local\bin\uvx.exe`, v0.11.8) |
| Google Cloud Console OAuthクライアント | ✅ 作成済み(Desktop app, Docs/Drive スコープ設定済み, テストユーザー登録済み) |
| google-workspace MCP接続 | ✅ `tools/list` が37ツールを約1秒で返す(既知の問題2は解決) |
| OAuth認証(個人) | ✅ `tenmaimp@gmail.com` |
| OAuth認証(大学) | ✅ `tenma_uchiyama@keio.jp`(**アンダースコア**。ハイフンではない) |
| Google Docs/Driveの読み取り | ✅ `search_drive_files` / `get_doc_content` で実データ取得を確認 |
| Google Docsへの書き込み | ✅ `create_doc`(新規作成)→ `batch_update_doc`(既存文書へ追記)→ 読み戻し、の往復を確認 |
| PUCドキュメントへのアクセス | ✅ Keioアカウントで取得。ID `1nZV32YmGqC6nMffWHaM81Mem6PggSRnX6lakOi3XpEQ` |

Keio(Google Workspace組織)アカウントも、Test usersに追加すれば問題なく認証できた。組織の管理者ポリシーによる第三者アプリのブロックは、少なくとも2026-08-10時点のKeioアカウントでは発生しなかった。

### PUCドキュメント

```
ID:   1nZV32YmGqC6nMffWHaM81Mem6PggSRnX6lakOi3XpEQ
所有: tenma_uchiyama@keio.jp
URL:  https://docs.google.com/document/d/1nZV32YmGqC6nMffWHaM81Mem6PggSRnX6lakOi3XpEQ/edit
```

### 書き込み時のTips

- 既存文書への追記は `batch_update_doc` に `{"type":"insert_text","text":"...","end_of_segment":true}` を渡す(`index` は省略する)。
- 書式設定や特定位置への挿入を行う場合は、先に `inspect_doc_structure` で正確なインデックスを取得すること。インデックスを当て推量で指定しないこと。
- ヘッダー/フッターの `segment_id` も同様に `inspect_doc_structure` の返り値を使う(`kix.header` のような値を推測しない)。

### 運用上の注意

- **サーバーはOS再起動で落ちる。** 再起動後は2-3節の起動コマンドを実行し直すこと。
- **リフレッシュトークンは7日で失効する**(External + Testing ステータスのため)。失効したら2-4節の認証をやり直す。
- 読み取り専用でよければ、既存の `claude.ai Google Drive`(claude.ai公式コネクタ、追加セットアップ不要)も引き続き使える。
