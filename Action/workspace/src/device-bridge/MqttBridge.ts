import Debugger from "@/debugger/Debugger";
import { IRxData } from "@/types/ActionBlockInterfaces";
import { EventEmitter } from "events";
import mqtt, { MqttClient } from "mqtt";

export default class MqttBridge extends EventEmitter {
  private static instance: MqttBridge;
  private subscribedTopicsMap: Map<string, Set<(data: string) => void>> =
    new Map();
  private mqttClient: MqttClient;

  // Constructor is now private to enforce the singleton pattern
  private constructor(brokerUrl: string) {
    super();
    this.mqttClient = mqtt.connect(brokerUrl);
    this.setupMqtt();
  }

  // Singleton instance accessor with optional broker URL parameter
  public static getInstance(
    //mqtt://mqtt-broker:1883
    brokerUrl: string = "mqtt://192.168.10.102:1883"
  ): MqttBridge {
    if (!MqttBridge.instance) {
      MqttBridge.instance = new MqttBridge(brokerUrl);
    }
    return MqttBridge.instance;
  }

  private setupMqtt() {
    this.mqttClient.on("connect", () => {
      console.log("[MQTT BRIDGE] Connected to MQTT broker");
    });

    // Message event handler is set up once
    this.mqttClient.on("message", this.handleMessage.bind(this));
  }

  // Helper method to normalize topic names
  private normalizeTopic(topic: string): string {
    return topic.startsWith("/") ? `bridge${topic}` : `bridge/${topic}`;
  }

  // Centralized message handler
  private handleMessage(topic: string, message: Buffer) {
    console.log(
      `[BRIDGE] Received message from ${topic}: ${message.toString()}`
    );
    const handlers = this.subscribedTopicsMap.get(topic);
    if (handlers) {
      const payload = message.toString();
      console.log(`[BRIDGE] Received message from ${topic}: ${payload}`);
      handlers.forEach((handler) => handler(payload));
    }
  }

  public subscribeToTopic(topic: string, handler: (data: string) => void) {
    topic = this.normalizeTopic(topic);

    let handlers = this.subscribedTopicsMap.get(topic);
    if (!handlers) {
      handlers = new Set();
      this.subscribedTopicsMap.set(topic, handlers);

      this.mqttClient.subscribe(topic, (err) => {
        if (err) {
          console.error(`Failed to subscribe to topic ${topic}:`, err);
        } else {
          console.log(`Subscribed to topic: ${topic}`);
        }
      });
    }
    handlers.add(handler);
  }

  public unsubscribeFromTopic(topic: string, handler: (data: string) => void) {
    topic = this.normalizeTopic(topic);

    const handlers = this.subscribedTopicsMap.get(topic);
    if (handlers && handlers.delete(handler)) {
      console.log(`Handler removed from topic: ${topic}`);

      if (handlers.size === 0) {
        this.mqttClient.unsubscribe(topic, (err) => {
          if (err) {
            console.error(`Failed to unsubscribe from topic ${topic}:`, err);
          } else {
            console.log(`Unsubscribed from topic: ${topic}`);
            this.subscribedTopicsMap.delete(topic);
          }
        });
      }
    } else {
      console.warn(`Handler not found for topic: ${topic}`);
    }
  }

  public publishMessage(topic: string, message: string) {
    this.mqttClient.publish(topic, message, (err) => {
      if (err) {
        console.error(`Failed to publish message to ${topic}:`, err);
      }
    });
  }
}