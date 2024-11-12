import ActionBlock from "@block/ActionBlock";
import {
  IDeviceBlock,
  IDeviceData,
  ISignalData,
} from "@/types/ActionBlockInterfaces";
import MqttBridge from "@mqtt-bridge";
import Debugger from "@debugger/Debugger";
import { on } from "events";

export default class DeviceBlock extends ActionBlock implements IDeviceBlock {
  device_data: IDeviceData;
  device_data_id: string;
  topic: string;
  private data_handler: (data: string) => void;

  constructor(deviceBlockInitializers: IDeviceBlock) {
    super(deviceBlockInitializers);
    this.device_data = deviceBlockInitializers.device_data;
    this.topic = this.device_data.mqtt_topic;
    this.data_handler = this.onReceiveDataFromSensor.bind(this);

    this.sendRequestStatus();
  }

  startAction(): void {
    super.startAction();
    MqttBridge.getInstance().subscribeToTopic(this.topic, this.data_handler);
  }

  exitAction(): void {
    super.exitAction();
    MqttBridge.getInstance().unsubscribeFromTopic(
      this.topic,
      this.data_handler
    );
  }

  sendRequestStatus(): void {
    let request: ISignalData = {
      action_id: this.id,
      data_type: "request",
      value: "mrflow/" + this.id,
    };

    Debugger.getInstance().debugLog(
      this.getRoutineId(),
      "DEVICE",
      "Requesting status from device"
    );
    MqttBridge.getInstance().publishMessage(
      this.topic,
      JSON.stringify(request)
    );
  }

  onReceiveDataFromPreviousBlock(data: ISignalData): void {
    data.action_id = this.id;
    MqttBridge.getInstance().publishMessage(this.topic, JSON.stringify(data));
    // if (data.data_type === "trigger") {
    //   let request: ISignalData = {
    //     action_id: this.id,
    //     data_type: "request",
    //     value: "mrflow/" + this.id,
    //   };
    //   MqttBridge.getInstance().publishMessage(
    //     this.topic,
    //     JSON.stringify(request)
    //   );
    //   data.action_id = this.id;
    //   this.startNextActionBlock();
    //   this.senderDataStream?.next(data);
    // }

    // if (data.data_type === "boolean") {
    //   console.log("SEnding Data, ", data);
    //   const { action_id, data_type, value } = data;

    //   MqttBridge.getInstance().publishMessage(this.topic, JSON.stringify(data));

    //   data.action_id = this.id;
    //   this.startNextActionBlock();
    //   this.senderDataStream?.next(data);
    // }
  }

  onReceiveDataFromSensor(data: string): void {}
}
