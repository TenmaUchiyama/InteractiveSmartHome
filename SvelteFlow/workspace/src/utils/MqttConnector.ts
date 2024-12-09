import mqtt, { type MqttClient } from "mqtt";

export default class MqttBridge {
  private static instance: MqttBridge;

  private brokerUrl: string = "ws://localhost:9001";
  private subscribedTopicsMap: Map<string, Set<(data: string) => void>> =
    new Map();
  mqttClient: MqttClient | undefined;

  // Constructor is now private to enforce the singleton pattern
  private constructor() {
    this.setupMqtt();
  }

  // Singleton instance accessor with optional broker URL parameter
  public static getInstance(): MqttBridge {
    if (!MqttBridge.instance) {
      MqttBridge.instance = new MqttBridge();
    }
    return MqttBridge.instance;
  }

  public async setupMqtt() {
    this.mqttClient = await mqtt.connectAsync(this.brokerUrl);
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
    const handlers = this.subscribedTopicsMap.get(topic);
    if (handlers) {
      const payload = message.toString();
      handlers.forEach((handler) => handler(payload));
    }
  }

  public subscribeToTopic(topic: string, handler: (payload: string) => void) {
    if (!this.mqttClient) return;
    let handlers = this.subscribedTopicsMap.get(topic);
    if (!handlers) {
      handlers = new Set();
      this.subscribedTopicsMap.set(topic, handlers);

      this.mqttClient.subscribe(topic, (err: any) => {
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
    if (!this.mqttClient) return;

    topic = this.normalizeTopic(topic);

    const handlers = this.subscribedTopicsMap.get(topic);
    if (handlers && handlers.delete(handler)) {
      console.log(`Handler removed from topic: ${topic}`);

      if (handlers.size === 0) {
        this.mqttClient.unsubscribe(topic, (err: any) => {
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
}
