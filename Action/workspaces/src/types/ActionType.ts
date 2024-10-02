export enum ActionType {
  // Logic
  Logic_Timer = 'logic-timer',
  Logic_SimpleComparator = 'logic-simple-comparator',
  Logic_RangeComparator = 'logic-range-comparator',

  // Device
  Device = 'device',
}

export enum DeviceType {
  Sensor_Thermometer = 'sensor-thermometer',
  Sensor_ToggleButton = 'sensor-toggle-button',
  Sensor_Motion = 'sensor-motion',
  Actuator_Light = 'actuator-light',
}
