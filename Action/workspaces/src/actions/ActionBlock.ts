import { ActionEvents, ActionEventKey } from "./ActionEvents";
import DeviceSignalTransit from "../device-bridge/MqttBridge"; // 実際のファイルパスに置き換えてください
import MqttBridge from "../device-bridge/MqttBridge";
import { IDevice } from "src/types/ActionTypes";
export class ActionBlock {
  private id: string;
  private topic: string;
  private routineId: string;
  
  constructor(deviceData : IDevice) { 
    this.id = deviceData.id; 
    this.topic = deviceData.topic;
    this.routineId = "";
  }

  
  public getID(): string {
    return this.id;
  }

  public setRoutineID(routineId : string)
  {
    this.routineId = routineId;
  }
  public start(): void {

    console.log(`ActionBlock ${this.id} is starting.`);
    this.sendMessage('Hello World!')
    MqttBridge.getInstance().subscribeToTopic("bridge/"+this.topic, this.handleSensorData.bind(this))
  }

  
  private handleSensorData(data: string) {
    console.log(`ActionBlock handling data for topic ${this.topic}:`, data);
    console.log(this.routineId)
    ActionEvents.GetInstance().emit(`${ActionEventKey.ExitAction}_${this.routineId}`, this.id);


}

public sendMessage(message: string) {
    const bridge = MqttBridge.getInstance();
    MqttBridge.getInstance().publishMessage(this.topic, message);
}
}
