export enum ActionType {
  // Test
  Test = "test",

  // Logic
  Logic_Timer = "logic-timer",
  Logic_SimpleComparator = "logic-simple-comparator",
  Logic_RangeComparator = "logic-range-comparator",
  Logic_Gate = "logic-gate",
  Logic_NotGate = "logic-not-gate",
  Logic_Schedule = "logic-schedule",

  // Device
  Device = "device",
}

export enum DeviceType {
  Sensor_Thermometer = "sensor-thermometer",
  Sensor_ToggleButton = "sensor-toggle-button",
  Sensor_Motion = "sensor-motion",
  Actuator_Light = "actuator-light",
}

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
  action_id: string;
  data_type: "string" | "number" | "boolean" | "json" | "trigger";
  value: string | number | boolean | object | null;
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
  action_data: {
    id: `${string}-${string}-${string}-${string}-${string}`;
    name: string;
    description: string;
    action_type: ActionType;
  };
  waitTime: number;
  device_data: IDeviceData;
}

/////////////Logic Block/////////////////////

export interface ISimpleComparatorLogicBlock extends IActionBlock {
  operator: ">" | "<" | "=" | "!=" | ">=" | "<=";
  value: number;
}

export interface IRangeComparatorLogicBlock extends IActionBlock {
  operatorFrom: ">" | "<" | ">=" | "<=";
  operatorTo: ">" | "<" | ">=" | "<=";
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
