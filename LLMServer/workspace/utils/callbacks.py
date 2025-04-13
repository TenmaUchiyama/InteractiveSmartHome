import os
from langchain.callbacks.base import BaseCallbackHandler
from langchain.schema import LLMResult




class CustomCallbackHandler(BaseCallbackHandler):
    def __init__(self, relative_path: str):
        root_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), ".."))
        base_path = os.path.join(root_dir, relative_path)

        base_dir = os.path.dirname(base_path)
        base_name, ext = os.path.splitext(os.path.basename(base_path))
        if ext.lower() != ".md":
            ext = ".md"

        self.log_path = os.path.join(base_dir, base_name + ext)
        os.makedirs(base_dir, exist_ok=True)
        self._initialized = False  # ← 最初だけ初期化するフラグ

    def on_llm_start(self, serialized, prompts, **kwargs):
        # 初回だけファイル内容をリセット
        if not self._initialized:
            with open(self.log_path, 'w', encoding='utf-8') as f:
                f.write("# 🧠 LLM Markdown Log\n\n")
            self._initialized = True

        log_text = "## 🚀 送信されたプロンプト\n"
        for i, prompt in enumerate(prompts):
            log_text += f"### Prompt {i + 1}\n```\n{prompt}\n```\n\n"
        self._write_log(log_text)

    def on_llm_end(self, response: LLMResult, **kwargs):
        log_text = "## 📥 LLMからのレスポンス\n"
        try:
            content = response.generations[0][0].text
        except Exception as e:
            content = f"[レスポンスの抽出に失敗しました: {e}]"
        log_text += f"```\n{content}\n```\n\n"
        self._write_log(log_text)

    def _write_log(self, text: str):
        try:
            with open(self.log_path, 'a', encoding='utf-8') as f:
                f.write(text)
                f.flush()
        except Exception as e:
            print(f"[ログファイルへの書き込みに失敗]: {e}")

    def add_output(self, add_txt: str):
        log_text = "## ✍️ 手動追加ログ\n"
        log_text += f"```\n{add_txt}\n```\n\n"
        self._write_log(log_text)
