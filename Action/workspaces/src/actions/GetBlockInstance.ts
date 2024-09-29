import ActionBlock from './action-blocks/ActionBlock';
import { ComparatorLogicBlock, TimerLogicBlock } from './action-blocks/logic';
import {
  ToggleButtonSensorBlock,
  ThermometerSensorBlock,
} from './action-blocks/device/sensor';
import { LightActuatorBlock } from './action-blocks/device/actuator';
import { ActionType, DeviceType } from '../types/ActionType';
import { IDBDeviceData, IDeviceBlock } from '../types/ActionBlockInterfaces';
import { MongoDB } from '../database-connector/mongodb';

export default async function getActionBlock(
  actionBlockData: any,
): Promise<ActionBlock> {
  switch (actionBlockData.action_type) {
    case ActionType.Logic_Timer:
      return new TimerLogicBlock(actionBlockData);
    case ActionType.Logic_Comparator:
      return new ComparatorLogicBlock(actionBlockData);
    case ActionType.Device:
      return await getDeviceBlock(actionBlockData);
    default:
      console.error(
        `[GetBlockInstance] ActionBlock not found for: ${actionBlockData.action_type}`,
      );
  }
}

async function getDeviceBlock(
  actionBlockData: IDBDeviceData,
): Promise<ActionBlock> {
  const device_id = actionBlockData.device_data_id;
  const device_data = await MongoDB.getInstance().getDeviceData(device_id);

  const actionBlock: IDeviceBlock = {
    device_data: device_data,
    id: actionBlockData.id,
    name: actionBlockData.name,
    description: actionBlockData.description,
    action_type: actionBlockData.action_type,
  };

  switch (actionBlockData.device_type) {
    case DeviceType.Actuator_Light:
      return new LightActuatorBlock(actionBlock);
    case DeviceType.Sensor_ToggleButton:
      return new ToggleButtonSensorBlock(actionBlock);
    case DeviceType.Sensor_Thermometer:
      return new ThermometerSensorBlock(actionBlock);

    default:
      console.error(
        `[GetBlockInstance] ActionBlock not found for: ${actionBlockData.action_type}`,
      );
  }
}
