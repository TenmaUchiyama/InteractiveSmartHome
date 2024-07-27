import mqtt from 'mqtt';

const client = mqtt.connect('mqtt://mqtt-broker:1883');

client.on('connect', () => {
  console.log('Connected to MQTT broker');

  // サブスクライブ
  client.subscribe('test/topic', (err: Error | null) => {
    if (!err) {
      console.log('Subscribed to test/topic');
      
      // メッセージのパブリッシュ
      client.publish('test/topic', 'Hello MQTT');
    } else {
      console.log('Subscription error:', err.message);
    }
  });
});

client.on('message', (topic: string, message: Buffer) => {
  console.log(`Received message: ${message.toString()} on topic: ${topic}`);
});
