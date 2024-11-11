import Debugger from "@debugger/Debugger";
import { IDeviceBlock, ISignalData } from "@/types/ActionBlockInterfaces";
import DeviceBlock from "../DeviceBlock";

export default class ToggleButtonSensorBlock extends DeviceBlock {
  constructor(sensorBlockInitializers: IDeviceBlock) {
    super(sensorBlockInitializers);
  }

  onReceiveDataFromSensor(data: string) {
    Debugger.getInstance().debugLog(
      this.getRoutineId(),
      "TOGGLE",
      "Received data from toggle button sensor: " + data
    );
    this.startNextActionBlock();

    const jsonData = JSON.parse(data);
    const rxData: ISignalData = {
      action_id: this.id,
      data_type: jsonData.data_type,
      value: jsonData.value,
    };
    this.senderDataStream?.next(rxData);
  }
}
