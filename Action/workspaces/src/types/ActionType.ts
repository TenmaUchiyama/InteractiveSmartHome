export enum ActionType {
  // Test
  Test = 'test',

  // Logic
  Logic_Timer = 'logic-timer',
  Logic_SimpleComparator = 'logic-simple-comparator',
  Logic_RangeComparator = 'logic-range-comparator',
  Logic_Gate = 'logic-gate',
  Logic_NotGate = 'logic-not-gate',
  Logic_Schedule = 'logic-schedule',

  // Device
  Device = 'device',
}

export enum DeviceType {
  Sensor_Thermometer = 'sensor-thermometer',
  Sensor_ToggleButton = 'sensor-toggle-button',
  Sensor_Motion = 'sensor-motion',
  Actuator_Light = 'actuator-light',
}
