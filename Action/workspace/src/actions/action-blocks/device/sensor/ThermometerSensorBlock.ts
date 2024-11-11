import Debugger from "@debugger/Debugger";
import { IDeviceBlock, ISignalData } from "@/types/ActionBlockInterfaces";
import DeviceBlock from "../DeviceBlock";

export default class ThermometerSensorBlock extends DeviceBlock {
  constructor(sensorBlockInitializers: IDeviceBlock) {
    super(sensorBlockInitializers);
  }

  onReceiveDataFromSensor(data: string) {
    Debugger.getInstance().debugLog(
      this.getRoutineId(),
      "THERMOMETER",
      "Received data from thermometer sensor:" + data
    );

    const jsonData = JSON.parse(data);
    const rxData: ISignalData = {
      action_id: this.id,
      data_type: jsonData.data_type,
      value: jsonData.value as boolean,
    };
    this.senderDataStream?.next(rxData);
  }
}
