import { IDBDeviceBlock, IDeviceBlock } from '@/types/ActionBlockInterfaces';
import { MongoDB } from '@database';
import { ActionType, DeviceType } from '@/types/ActionType';
import ActionBlock from '@block/ActionBlock';
import {
  SimpleComparatorLogicBlock,
  RangeComparatorLogicBlock,
  TimerLogicBlock,
} from '@block/logic';
import {
  ToggleButtonSensorBlock,
  ThermometerSensorBlock,
  MotionSensorBlock,
} from '@block/device/sensor';
import { LightActuatorBlock } from '@block/device/actuator';

const actionBlockMap = {
  [ActionType.Logic_Timer]: TimerLogicBlock,
  [ActionType.Logic_SimpleComparator]: SimpleComparatorLogicBlock,
  [ActionType.Logic_RangeComparator]: RangeComparatorLogicBlock,
  [ActionType.Device]: getDeviceBlock,
};

const deviceBlockMap = {
  [DeviceType.Actuator_Light]: LightActuatorBlock,
  [DeviceType.Sensor_ToggleButton]: ToggleButtonSensorBlock,
  [DeviceType.Sensor_Thermometer]: ThermometerSensorBlock,
  [DeviceType.Sensor_Motion]: MotionSensorBlock,
};

export default async function getActionBlock(
  actionBlockData: any,
): Promise<ActionBlock> {
  const ActionBlockConstructor = actionBlockMap[actionBlockData.action_type];

  if (ActionBlockConstructor) {
    if (actionBlockData.action_type === ActionType.Device) {
      return await getDeviceBlock(actionBlockData);
    }
    return new ActionBlockConstructor(actionBlockData);
  }

  throw new Error(`Unknown action type: ${actionBlockData.action_type}`);
}

async function getDeviceBlock(
  actionBlockData: IDBDeviceBlock,
): Promise<ActionBlock> {
  const device_id = actionBlockData.device_data_id;
  const device_data = await MongoDB.getInstance().getDevice(device_id);

  const actionBlock: IDeviceBlock = {
    device_data: device_data,
    id: actionBlockData.id,
    name: actionBlockData.name,
    description: actionBlockData.description,
    action_type: actionBlockData.action_type,
  };

  if (device_data === null) {
    console.error(
      `[GetBlockInstance] DeviceData not found for: ${JSON.stringify(
        actionBlockData,
        null,
        2,
      )}`,
    );
  }

  const DeviceBlockConstructor = deviceBlockMap[actionBlockData.device_type];

  if (DeviceBlockConstructor) {
    return new DeviceBlockConstructor(actionBlock);
  }

  console.error(
    `[GetBlockInstance] ActionBlock not found for: ${actionBlockData.device_type}`,
  );
}
