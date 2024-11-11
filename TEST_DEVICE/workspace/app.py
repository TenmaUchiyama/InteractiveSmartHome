import paho.mqtt.client as mqtt
import os
from dotenv import load_dotenv

# 環境変数に基づいて .env ファイルをロード
env_file = ".env.development" if os.getenv("NODE_ENV") == "docker" else ".env.docker"
load_dotenv(dotenv_path=env_file)

# ブローカーURLの指定（デフォルトは "mqtt://mqtt-broker:1883"）
broker_url = os.getenv("MQTT_URL", "mqtt://mqtt-broker:1883")

print(f"Broker URL: {broker_url}")





# ブローカーURLの指定（ローカルブローカーやMQTTサーバーのURL）
broker_url = "test.mosquitto.org"  # 公開MQTTブローカーの例
topic = "test/topic"

# コールバック関数の設定
def on_connect(client, userdata, flags, rc):
    if rc == 0:
        print("Connected to MQTT broker")
        client.subscribe(topic)
    else:
        print("Failed to connect, return code %d\n", rc)

def on_message(client, userdata, msg):
    print(f"Received message on {msg.topic}: {msg.payload.decode()}")

def on_subscribe(client, userdata, mid, granted_qos):
    print(f"Subscribed to topic: {topic}")
    # トピックにメッセージを送信
    client.publish(topic, "Hello MQTT")

def on_log(client, userdata, level, buf):
    print(f"Log: {buf}")



def StartConnection()
client = mqtt.Client()

# コールバックの設定
client.on_connect = on_connect
client.on_message = on_message
client.on_subscribe = on_subscribe
client.on_log = on_log

# MQTTブローカーに接続
client.connect(broker_url, 1883, 60)

# 通信開始
client.loop_forever()
