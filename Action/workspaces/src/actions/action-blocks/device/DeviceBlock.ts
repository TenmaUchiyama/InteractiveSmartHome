import ActionBlock from '@block/ActionBlock';
import { IDeviceBlock, IDeviceData } from '@/types/ActionBlockInterfaces';
import MqttBridge from '@mqtt-bridge';
import Debugger from '@debugger/Debugger';

export default class DeviceBlock extends ActionBlock implements IDeviceBlock {
  device_data: IDeviceData;
  device_data_id: string;
  topic: string;

  constructor(deviceBlockInitializers: IDeviceBlock) {
    super(deviceBlockInitializers);
    this.device_data = deviceBlockInitializers.device_data;
    this.topic = this.device_data.mqtt_topic;
  }

  startAction(): void {
    MqttBridge.getInstance().subscribeToTopic(
      this.topic,
      this.onReceiveDataFromSensor.bind(this),
    );
  }

  exitAction(): void {
    MqttBridge.getInstance().unsubscribeFromTopic(this.topic);
  }

  onReceiveDataFromSensor(data: string) {}
}
