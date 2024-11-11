import Debugger from "@debugger/Debugger";
import MqttBridge from "@mqtt-bridge";
import { IDeviceBlock, ISignalData } from "@/types/ActionBlockInterfaces";
import DeviceBlock from "../DeviceBlock";

export default class LightActuatorBlock extends DeviceBlock {
  constructor(sensorBlockInitializers: IDeviceBlock) {
    super(sensorBlockInitializers);
  }

  onReceiveDataFromPreviousBlock(data: ISignalData): void {
    Debugger.getInstance().debugLog(
      this.getRoutineId(),
      "LIGHT ACTUATOR BLOCK",
      "received data from previous block" + JSON.stringify(data)
    );

    if (data.data_type === "trigger") {
      MqttBridge.getInstance().publishMessage(this.topic, "true");
      data.action_id = this.id;
      this.startNextActionBlock();
      this.senderDataStream?.next(data);
      Debugger.getInstance().debugLog(
        this.getRoutineId(),
        "LIGHT ACTUATOR BLOCK",
        "sending data to " + this.topic
      );
    }
    if (data.data_type === "boolean") {
      console.log("SEnding Data, ", data);
      const { action_id, data_type, value } = data;

      Debugger.getInstance().debugLog(
        this.getRoutineId(),
        "LIGHT ACTUATOR BLOCK",
        "sending data to " + this.topic
      );
      MqttBridge.getInstance().publishMessage(
        this.topic,
        JSON.stringify({
          data_type: data_type,
          value: value,
        })
      );

      let informData: ISignalData = {
        action_id: this.id,
        data_type: "boolean",
        value: value,
      };

      data.action_id = this.id;
      this.startNextActionBlock();
      this.senderDataStream?.next(data);
    }
  }

  onReceiveDataFromSensor(data: string): void {
    Debugger.getInstance().debugLog(
      this.getRoutineId(),
      "LIGHT ACTUATOR BLOCK",
      "Received data from sensor: " + data
    );

    const jsonData = JSON.parse(data);
    const rxData: ISignalData = {
      action_id: this.id,
      data_type: jsonData.data_type,
      value: jsonData.value,
    };

    let sendingData: string = JSON.stringify(rxData);

    let informTopic = "mrflow/" + this.id;
    Debugger.getInstance().debugLog(
      this.getRoutineId(),
      "LIGHT ACTUATOR BLOCK",
      "Sending Topic: " + informTopic
    );
    MqttBridge.getInstance().publishMessage(informTopic, sendingData);
  }
}
