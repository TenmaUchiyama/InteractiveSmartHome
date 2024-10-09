export enum NodeType {
  Timer = "node-timer",
  SimpleComparator = "node-simple-comparator",
  RangeComparator = "node-range-comparator",
  GateLogic = "node-gate-logic",
  NotGate = "node-not-gate",
  Scheduler = "node-scheduler",

  Light = "node-light",
  ToggleButton = "node-toggle-button",
  Thermometer = "node-thermo-sensor",
}

export interface IDBNode {
  id: string;
  type: NodeType;
  data_action_id: string;
  position: { x: number; y: number };
}

export interface IEdge {
  id: string;
  source: string;
  target: string;
}

export interface IDBEdge {
  id: string;
  associated_routine_id: string;
  edges: IEdge[];
}
