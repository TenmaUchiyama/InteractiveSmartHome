import MqttBridge from '../../../../device-bridge/MqttBridge';
import { IDeviceBlock } from '../../../../types/ActionBlockInterfaces';
import DeviceBlock from '../DeviceBlock';

export default class LightActuatorBlock extends DeviceBlock {
  constructor(sensorBlockInitializers: IDeviceBlock) {
    super(sensorBlockInitializers);
  }

  onReceiveDataFromPreviousBlock(data: any): void {
    console.log(`[LIGHT ACTUATOR] Sending data to topic ${this.topic}:`, data);
    MqttBridge.getInstance().publishMessage(this.topic, data);
  }
}
