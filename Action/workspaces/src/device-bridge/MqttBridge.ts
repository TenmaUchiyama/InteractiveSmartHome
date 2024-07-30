
import { EventEmitter } from "events";
import mqtt, { MqttClient } from "mqtt";


type TopicHandler = (data:string) => void;

export default class MqttBridge extends EventEmitter{
  public static instance: MqttBridge = new MqttBridge();


  private mqttClient: MqttClient;
  private mqttTopic: string = "bridge";


  private topicHandlers: { [topic: string]: TopicHandler[] } = {};


  public static getInstance(): MqttBridge {
    if (!MqttBridge.instance) {
        
        MqttBridge.instance = new MqttBridge();
    }
    return MqttBridge.instance;
}
  constructor() {
    super()

    this.mqttClient = mqtt.connect('mqtt://mqtt-broker:1883');
    this.setupMqtt();
  }

  private setupMqtt() {
    this.mqttClient.on('connect', () => {
        console.log('Connected to MQTT broker');
    });

    this.mqttClient.on('message', (topic, message) => {
        const payload = message.toString();
        this.emitMessageToHandlers(topic, payload);
    });
}

  
public subscribeToTopic(topic: string, handler: TopicHandler) {
  if (!this.topicHandlers[topic]) {
      this.topicHandlers[topic] = [];
      this.mqttClient.subscribe(topic, (err) => {
          if (err) {
              console.error(`Failed to subscribe to topic ${topic}:`, err);
          } else {
              console.log(`Subscribed to topic: ${topic}`);
          }
      });
  }
  this.topicHandlers[topic].push(handler);
}


public publishMessage(topic: string, message: string) {
  this.mqttClient.publish(topic, message, (err) => {
      if (err) {
          console.error(`Failed to publish message to ${topic}:`, err);
      } else {
          console.log(`Message published to ${topic}: ${message}`);
      }
  });
}


  private emitMessageToHandlers(topic: string, message: string) {
    const handlers = this.topicHandlers[topic];
    if (handlers) {
        handlers.forEach(handler => handler(message));
    }
}
}
