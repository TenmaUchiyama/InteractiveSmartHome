import { ActionEvents, ActionEventKey } from "./ActionEvents";
import DeviceSignalTransit from "../device-bridge/MqttBridge"; // 実際のファイルパスに置き換えてください
import MqttBridge from "../device-bridge/MqttBridge";

export class ActionBlock {
  private id: string;
  private topic: string;

  
  constructor(id: string , topic: string) { 
    this.id = id;
    this.topic = topic;
  }

  
  public getID(): string {
    return this.id;
  }
  public start(): void {
    console.log(`ActionBlock ${this.id} is starting.`);
    this.sendMessage('Hello World!')
    MqttBridge.getInstance().subscribeToTopic("bridge/"+this.topic, this.handleSensorData.bind(this));
  }





  private handleSensorData(data: string) {
    console.log(`ActionBlock handling data for topic ${this.topic}:`, data);

  ActionEvents.GetInstance().emit(ActionEventKey.ExitAction, this.id);
}

public sendMessage(message: string) {
    const bridge = MqttBridge.getInstance();
    MqttBridge.getInstance().publishMessage(this.topic, message);
}
}
