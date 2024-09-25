import { EventEmitter } from 'events';
import mqtt, { MqttClient } from 'mqtt';

/**
 * This module bridges between the application to the MQTT broker for communication with the devices.
 */

export default class MqttBridge extends EventEmitter {
  public static instance: MqttBridge = new MqttBridge();

  private mqttClient: MqttClient;
  private mqttTopic: string = 'bridge';

  public static getInstance(): MqttBridge {
    if (!MqttBridge.instance) {
      MqttBridge.instance = new MqttBridge();
    }
    return MqttBridge.instance;
  }
  constructor() {
    super();

    this.mqttClient = mqtt.connect('mqtt://mqtt-broker:1883');
    this.setupMqtt();
  }

  private setupMqtt() {
    this.mqttClient.on('connect', () => {
      console.log('Connected to MQTT broker');
    });
  }

  public subscribeToTopic(topic: string, handler: (data: string) => void) {
    this.mqttClient.subscribe(topic, (err) => {
      if (err) {
        console.error(`Failed to subscribe to topic ${topic}:`, err);
      } else {
        console.log(`Subscribed to topic: ${topic}`);

        this.mqttClient.on('message', (topic, message) => {
          const payload = message.toString();
          console.log(`Received message from ${topic}: ${payload}`);
          handler(payload);
        });
      }
    });
  }

  public unsubscribeFromTopic(topic: string) {
    this.mqttClient.unsubscribe(topic, (err) => {
      if (err) {
        console.error(`Failed to unsubscribe from topic ${topic}:`, err);
      } else {
        console.log(`Unsubscribed from topic: ${topic}`);
      }
    });
  }

  public async publishMessage(topic: string, message: string) {
    this.mqttClient.publish(topic, message, (err) => {
      if (err) {
        console.error(`Failed to publish message to ${topic}:`, err);
      } else {
        console.log(`Message published to ${topic}: ${message}`);
      }
    });
  }
}
