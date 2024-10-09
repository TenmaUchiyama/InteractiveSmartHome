import Debugger from '@debugger/Debugger';
import MqttBridge from '@mqtt-bridge';
import { IDeviceBlock, IRxData } from '@/types/ActionBlockInterfaces';
import DeviceBlock from '../DeviceBlock';

export default class LightActuatorBlock extends DeviceBlock {
  constructor(sensorBlockInitializers: IDeviceBlock) {
    super(sensorBlockInitializers);
  }

  onReceiveDataFromPreviousBlock(data: IRxData): void {
    Debugger.getInstance().debugLog(
      this.getRoutineId(),
      'LIGHT ACTUATOR BLOCK',
      'received data from previous block' + JSON.stringify(data),
    );

    if (data.data_type === 'trigger') {
      MqttBridge.getInstance().publishMessage(this.topic, 'true');
      data.action_id = this.id;
      this.startNextActionBlock();
      this.senderDataStream?.next(data);
      Debugger.getInstance().debugLog(
        this.getRoutineId(),
        'LIGHT ACTUATOR BLOCK',
        'sending data to ' + this.topic,
      );
    }
    if (data.data_type === 'boolean') {
      console.log('SEnding Data, ', data);
      const { action_id, data_type, value } = data;

      Debugger.getInstance().debugLog(
        this.getRoutineId(),
        'LIGHT ACTUATOR BLOCK',
        'sending data to ' + this.topic,
      );
      MqttBridge.getInstance().publishMessage(
        this.topic,
        JSON.stringify({
          data_type: data_type,
          value: value,
        }),
      );

      data.action_id = this.id;
      this.startNextActionBlock();
      this.senderDataStream?.next(data);
    }
  }
}
