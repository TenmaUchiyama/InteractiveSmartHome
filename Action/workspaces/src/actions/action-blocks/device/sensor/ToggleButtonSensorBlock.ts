import { IDeviceBlock } from '../../../../types/ActionBlockInterfaces';
import DeviceBlock from '../DeviceBlock';

export default class ToggleButtonSensorBlock extends DeviceBlock {
  constructor(sensorBlockInitializers: IDeviceBlock) {
    super(sensorBlockInitializers);
  }

  onReceiveDataFromSensor(data: any) {
    console.log('[TOGGLE]Received data from toggle button sensor:', data);
    this.startNextActionBlock();
    this.senderDataStream?.next(data);
  }
}
