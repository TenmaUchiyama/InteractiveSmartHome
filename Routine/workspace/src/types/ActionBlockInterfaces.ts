import { ActionType, DeviceType } from "./ActionType";

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

export interface ISignalData {
  action_id: string;
  data_type:
    | "string"
    | "number"
    | "boolean"
    | "json"
    | "trigger"
    | "request"
    | "init";
  value: string | number | boolean | object | null;
  metadata?: object;
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
  device_position: {
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

export enum Comparator {
  GREATER_THAN = ">",
  LESS_THAN = "<",
  EQUAL = "=",
  NOT_EQUAL = "!=",
  GREATER_THAN_OR_EQUAL = ">=",
  LESS_THAN_OR_EQUAL = "<=",
}

export interface ISimpleComparatorLogicBlock extends IActionBlock {
  comparator: Comparator;
  value: number;
}

export interface IRangeComparatorLogicBlock extends IActionBlock {
  comparatorFrom: ">" | "<" | ">=" | "<=";
  comparatorTo: ">" | "<" | ">=" | "<=";
  from: number;
  to: number;
}
export interface ITimerLogicBlock extends IActionBlock {
  waitTime: number;
}

export interface IGateLogicBlock extends IActionBlock {
  logic_operator: "AND" | "OR";
}

export interface INotGateLogicBlock extends IActionBlock {
  logic_operator: "NOT";
}
export interface IScheduleLogicBlock extends IActionBlock {
  cronExpression: string;
}
