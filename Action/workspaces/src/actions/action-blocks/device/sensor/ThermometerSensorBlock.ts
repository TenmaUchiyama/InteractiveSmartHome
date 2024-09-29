import { IDeviceBlock } from '../../../../types/ActionBlockInterfaces';
import DeviceBlock from '../DeviceBlock';

export default class ThermometerSensorBlock extends DeviceBlock {
  constructor(sensorBlockInitializers: IDeviceBlock) {
    super(sensorBlockInitializers);
  }

  onReceiveDataFromSensor(data: any) {
    console.log('[THERMOMETER]Received data from thermometer sensor:', data);

    this.senderDataStream?.next(data);
  }
}
