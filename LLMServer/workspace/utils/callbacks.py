import os
from langchain_core.callbacks.base import BaseCallbackHandler
from langchain_core.outputs.llm_result import LLMResult

class LogFileWriter:
    """
    ファイルへの書き込みを担うクラス。
    指定されたファイルパスに対してログ内容を書き込みます。
    """
    def __init__(self, file_path: str):
        self.file_path = file_path
        # 書き込み先のディレクトリが存在しない場合は作成
        base_dir = os.path.dirname(self.file_path)
        os.makedirs(base_dir, exist_ok=True)
    
    def write_log(self, text: str):
        try:
            with open(self.file_path, 'a', encoding='utf-8') as f:
                f.write(text)
                f.flush()
        except Exception as e:
            print(f"[ログファイルへの書き込みに失敗]: {e}")

class CustomCallbackHandler(BaseCallbackHandler):
    def __init__(self, relative_path: str):
        # ルートディレクトリからの絶対パスを算出
        root_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
        base_path = os.path.join(root_dir, relative_path)

        base_dir = os.path.dirname(base_path)
        base_name, ext = os.path.splitext(os.path.basename(base_path))
        if ext.lower() != ".md":
            ext = ".md"

        self.log_path = os.path.join(base_dir, base_name + ext)
        
        # LogFileWriter のインスタンスを生成し、ファイル書き込みを委譲
        self.file_writer = LogFileWriter(self.log_path)
        self._initialized = False  # 初回だけ初期化するフラグ

    def on_llm_start(self, serialized, prompts, **kwargs):
        # 初回のみファイル内容をリセット
        if not self._initialized:
            with open(self.log_path, 'w', encoding='utf-8') as f:
                f.write("# 🧠 LLM Markdown Log\n\n")
            self._initialized = True

        log_text = "## 🚀 送信されたプロンプト\n"
        for i, prompt in enumerate(prompts):
            log_text += f"### Prompt {i + 1}\n\n{prompt}\n\n\n"
        self.file_writer.write_log(log_text)

    def on_llm_end(self, response: LLMResult, **kwargs):
        log_text = "## 📥 LLMからのレスポンス\n"
        try:
            content = response.generations[0][0].text
        except Exception as e:
            content = f"[レスポンスの抽出に失敗しました: {e}]"
        log_text += f"\n{content}\n\n\n"
        self.file_writer.write_log(log_text)

    def add_output(self, add_txt: str):
        log_text = "## ✍️ 手動追加ログ\n"
        log_text += f"\n{add_txt}\n\n\n"
        self.file_writer.write_log(log_text)

    def add_mid(self, caller :str, add_txt: str):
        log_text = f"## ✍️ {caller}\n"
        log_text += f"\n{add_txt}\n\n\n"
        self.file_writer.write_log(log_text)
