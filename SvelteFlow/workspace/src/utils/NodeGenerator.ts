import {
  ActionType,
  DeviceType,
  type IDBDeviceBlock,
  type ITimerLogicBlock,
} from "@/type/ActionBlockInterface";
import { NodeType } from "@/type/NodeType";
import { type Node } from "@xyflow/svelte";

const actionDataTemplate: any = {
  [NodeType.Timer]: {
    waitTime: 0,
    id: crypto.randomUUID(),
    name: "Timer Template",
    description: "This is a timer template",
    action_type: ActionType.Logic_Timer,
  },
  [NodeType.SimpleComparator]: {
    comperator: ">",
    value: 0,
    id: crypto.randomUUID(),
    name: "Simple Comparator Template",
    description: "This is a simple comparator template",
    action_type: ActionType.Logic_SimpleComparator,
  },
  [NodeType.RangeComparator]: {
    comparatorFrom: ">",
    comparatorTo: "<",
    from: 0,
    to: 0,
    id: crypto.randomUUID(),
    name: "Range Comparator Template",
    description: "This is a range comparator template",
    action_type: ActionType.Logic_RangeComparator,
  },
  [NodeType.GateLogic]: {
    logic_operator: "AND",
    id: crypto.randomUUID(),
    name: "Gate Logic Template",
    description: "This is a gate logic template",
    action_type: ActionType.Logic_Gate,
  },
  [NodeType.NotGate]: {
    logic_operator: "NOT",
    id: crypto.randomUUID(),
    name: "Not Gate Template",
    description: "This is a not gate template",
    action_type: ActionType.Logic_NotGate,
  },
  [NodeType.Scheduler]: {
    id: crypto.randomUUID(),
    name: "Scheduler Template",
    description: "This is a scheduler template",
    action_type: ActionType.Logic_Schedule,
  },
  [NodeType.Light]: {
    device_type: DeviceType.Actuator_Light,
    id: crypto.randomUUID(),
    name: "Light Template",
    description: "This is a light template",
    action_type: ActionType.Device,
    device_data_id: "led-sample-1",
  } satisfies IDBDeviceBlock,

  [NodeType.ToggleButton]: {
    id: crypto.randomUUID(),
    name: "Toggle Button Template",
    description: "This is a toggle button template",
    action_type: ActionType.Device,
    device_data_id: "toggle-device-1",
    device_type: DeviceType.Sensor_ToggleButton,
  } satisfies IDBDeviceBlock,
  [NodeType.Thermometer]: {
    device_type: DeviceType.Sensor_Thermometer,
    id: crypto.randomUUID(),
    name: "Thermo Sensor Template",
    description: "This is a thermo sensor template",
    action_type: ActionType.Device,
    device_data_id: "thermo1",
  },
};

export const generateNode = (node: NodeType): Node => {
  let newNode: Node = {
    id: crypto.randomUUID(),
    type: node,
    data: { action_data: null },
    position: { x: 0, y: 0 },
  };

  // Deep copy the action data template to avoid reference issues
  let newBlock = JSON.parse(JSON.stringify(actionDataTemplate[node]));

  newNode.data = { action_data: newBlock };
  console.log(newNode.data.action_data);
  console.log(actionDataTemplate);

  return newNode;
};

export const logics = [
  { type: NodeType.Timer, label: "Timer" },
  { type: NodeType.SimpleComparator, label: "SimpleComparator" },
  { type: NodeType.RangeComparator, label: "RangeComparator" },
  { type: NodeType.Scheduler, label: "Scheduler" },
  { type: NodeType.NotGate, label: "NotGate" },
  { type: NodeType.GateLogic, label: "GateLogic" },
];

export const sensors = [
  { type: NodeType.Thermometer, label: "Thermometer" },
  { type: NodeType.ToggleButton, label: "ToggleButton" },
];

export const actuators = [{ type: NodeType.Light, label: "Light" }];
