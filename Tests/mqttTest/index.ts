import * as mqtt from "mqtt";

const host = "192.168.10.102";
const port = "1883";
const clientId = `mqtt_${Math.random().toString(16).slice(3)}`;

const connectUrl = `mqtt://${host}:${port}`;

const client = mqtt.connect(connectUrl, {
  clientId,
  clean: true,
  connectTimeout: 4000,
  username: "your_username", // 必要に応じてMQTTサーバーのユーザー名を設定
  password: "your_password", // 必要に応じてMQTTサーバーのパスワードを設定
  reconnectPeriod: 1000,
});

client.on("connect", () => {
  console.log("Connected");

  // メッセージを送信する
  const topic = "test/topic";
  const message = "hello pippi";

  client.publish(topic, message, { qos: 0, retain: false }, (error) => {
    if (error) {
      console.error(error);
    } else {
      console.log(`Message sent: ${message}`);
    }
    // 接続を閉じる
    client.end();
  });
});

client.on("error", (err) => {
  console.error("Connection error: ", err);
  client.end();
});
