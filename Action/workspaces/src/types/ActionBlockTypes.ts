import { DeviceData } from './DBTypes';

export type ActionBlockType = {
  id: string;
  name: string;
  description: string;
};

export type SensorDeviceType = ActionBlockType & {
  topic: string;
  valueType: 'binary' | 'value';
};

export type ActuatorDeviceType = ActionBlockType & {
  topic: string;
  valueType: 'binary' | 'value';
};

export type TimerLogicType = ActionBlockType & {
  time: string;
};

// export type ConditionType  = ActionBlockType & {
//   conditions: Condition[];

// }
