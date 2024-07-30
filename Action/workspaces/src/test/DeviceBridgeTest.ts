import { EventEmitter } from "stream";
import mqtt, { MqttClient } from "mqtt";

export class BridgeTest extends EventEmitter 
{
    private mqttClient: MqttClient;

    public static instance: BridgeTest = new BridgeTest();

    constructor() {
        super()

        console.log("BridgeTest constructor");

        this.mqttClient = mqtt.connect('mqtt://mqtt-broker:1883');
        this.setupMqtt();
    }
  
    private setupMqtt() {
      this.mqttClient.on('connect', () => {
          console.log('Connected to MQTT broker');
      });
  
      this.mqttClient.on('message', (topic, message) => {
          const payload = message.toString();
          console.log("======== PayLoad ==========", payload)
      });
  }
  
    public static getInstance(): BridgeTest {
        if (!BridgeTest.instance) {
            console.log("Creating new [DeviceBridge] instance");
            BridgeTest.instance = new BridgeTest();
        }
        return BridgeTest.instance;
    }
    

    
}