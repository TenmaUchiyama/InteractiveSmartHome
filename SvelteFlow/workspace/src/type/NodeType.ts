export enum NodeType {
  Timer = "node-timer",
  SimpleComparator = "node-simple-comparator",
  RangeComparator = "node-range-comparator",
  GateLogic = "node-gate-logic",
  NotGate = "node-not-gate",

  Light = "node-light",
  ToggleButton = "node-toggle-button",
  Thermometer = "node-thermo-sensor",

  Scheduler = "node-scheduler",
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

export interface RoutineEdge {
  id: string;
  associated_routine_id: string;
  routine_name: string;
  edges: IEdge[];
  nodes: string[];
}

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
