import ActionBlock from '@block/ActionBlock';
import { IDeviceBlock, IDeviceData } from '@/types/ActionBlockInterfaces';
import MqttBridge from '@mqtt-bridge';
import Debugger from '@debugger/Debugger';

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
  }

  startAction(): void {
    super.startAction();
    MqttBridge.getInstance().subscribeToTopic(this.topic, this.data_handler);
  }

  exitAction(): void {
    super.exitAction();
    MqttBridge.getInstance().unsubscribeFromTopic(
      this.topic,
      this.data_handler,
    );
  }

  onReceiveDataFromSensor(data: string) {}
}
