import paho.mqtt.client as mqtt
import json

# MQTTブローカーの設定
BROKER = "localhost"  # MQTTブローカーのアドレス（例：localhost）
PORT = 1883           # MQTTポート（通常は1883）
TOPIC = "device/1077106d-c380-4967-923b-c112823eaf88"  # 送信するトピック

# 送信するデータ
data = {
    "id": "device1",
    "state": False,
    "intensity": 75,
    "color": {"r": 255, "g": 120, "b": 60}
}

# JSON形式に変換
payload = json.dumps(data)

# MQTTクライアントの作成
client = mqtt.Client()

# MQTTブローカーに接続
client.connect(BROKER, PORT, 60)

# メッセージを送信
client.publish(TOPIC, payload)

print(f"Published to {TOPIC}: {payload}")

# 接続を終了
client.disconnect()
