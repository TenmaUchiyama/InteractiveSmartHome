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

    super.onReceiveDataFromPreviousBlock(data);
  }

  onReceiveDataFromSensor(data: string): void {
    Debugger.getInstance().debugLog(
      this.getRoutineId(),
      "LIGHT ACTUATOR BLOCK",
      "Received data from sensor: " + data
    );

    // super.onReceiveDataFromSensor(data);
  }
}
