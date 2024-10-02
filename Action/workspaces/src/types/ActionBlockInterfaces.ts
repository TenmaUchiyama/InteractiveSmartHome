import { ActionType, DeviceType } from './ActionType';

/////////////Routine/////////////////////
export interface IRoutine {
  first?: boolean;
  last?: boolean;
  current_block_id: string;
  next_block_id: string;
}

export interface IRoutineData {
  id: string;
  name: string;
  action_routine: IRoutine[];
}

/////////////Action Block/////////////////////

export interface IRxData {
  data_type: 'string' | 'number' | 'boolean' | 'json';
  value: string | number | boolean | object;
}
export interface IActionBlock {
  id: string;
  name: string;
  description: string;
  action_type: ActionType;
}

/////////////Device Block/////////////////////
export interface IDeviceData {
  device_id: string;
  device_name: string;
  device_type: string;
  mqtt_topic: string;
  device_location: {
    x: number;
    y: number;
    z: number;
  };
}

export interface IDBDeviceBlock extends IActionBlock {
  device_data_id: string;
  device_type: DeviceType;
}

export interface IDeviceBlock extends IActionBlock {
  device_data: IDeviceData;
}

/////////////Logic Block/////////////////////
type ComparatorOperator = '>' | '<' | '=' | '!=' | '>=' | '<=';

export interface ISimpleComparatorLogicBlock extends IActionBlock {
  operator: ComparatorOperator;
  value: number;
}

export interface IRangeComparatorLogicBlock extends IActionBlock {
  operatorFrom: ComparatorOperator;
  operatorTo: ComparatorOperator;
  from: number;
  to: number;
}
export interface ITimerLogicBlock extends IActionBlock {
  waitTime: number;
}
