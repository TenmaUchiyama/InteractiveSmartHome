import json
import paho.mqtt.client as mqtt

class MQTTPublisher:
    def __init__(self, broker_address, port=1883):
        """
        コンストラクタでブローカーに接続します。
        :param broker_address: MQTTブローカーのアドレス
        :param port: MQTTブローカーのポート（デフォルトは1883）
        """
        self.broker_address = broker_address
        self.port = port
        self.client = mqtt.Client()

        # 接続時のコールバック設定
        self.client.on_connect = self.on_connect
        self.client.on_publish = self.on_publish

        # ブローカーに接続
        self.client.connect(self.broker_address, self.port, 60)

    def on_connect(self, client, userdata, flags, rc):
        """
        MQTTブローカーに接続したときのコールバック
        :param client: MQTTクライアントインスタンス
        :param userdata: ユーザーデータ（今回は使用しません）
        :param flags: 接続フラグ（今回は使用しません）
        :param rc: 接続結果
        """
        if rc == 0:
            print("Connected to MQTT broker successfully.")
        else:
            print(f"Failed to connect with result code {rc}")

    def on_publish(self, client, userdata, mid):
        """
        メッセージが公開されたときのコールバック
        :param client: MQTTクライアントインスタンス
        :param userdata: ユーザーデータ（今回は使用しません）
        :param mid: メッセージID
        """
        print(f"Message published with ID: {mid}")

    def send_data(self, topic, payload):
        """
        指定されたトピックにデータを送信します。
        :param topic: MQTTトピック
        :param payload: 送信するデータ
        """
        self.client.loop_start()  # 非同期でメインループを開始
        result = self.client.publish(topic, json.dumps(payload))  # データの送信
        print("[RESULT]: ", result)
        if result.rc != mqtt.MQTT_ERR_SUCCESS:
            return False
        self.client.loop_stop()  # メインループを停止
        return True




mqtt_publisher = MQTTPublisher("localhost")









# 使用例
if __name__ == "__main__":
    mqtt_publisher =  MQTTPublisher("localhost")   # MQTTブローカーのアドレス（例）
    mqtt_publisher.send_data("device/Device", "Hello MQTT From LLM Server")  # トピックとデータを指定して送信
