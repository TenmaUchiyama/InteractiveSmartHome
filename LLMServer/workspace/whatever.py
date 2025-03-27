from io import BytesIO
from IPython.display import Image, display
from agent_runner import runner
import PIL.Image

if __name__ == "__main__":
    # 画像のバイナリデータを取得
    img_data = runner.get_graph().draw_mermaid_png()

    # バイナリデータをBytesIOに変換
    img_io = BytesIO(img_data)

    # PILで開く
    img = PIL.Image.open(img_io)
    img.show()