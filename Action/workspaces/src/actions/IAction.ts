export enum ActionType {
  SENSOR = "SENSOR",
  ACTUATOR = "ACTUATOR",
  LOGIC = "LOGIC",
  LLM = "LLM",
}

export interface IActions {
  id: string;
  topic: string;
  type: ActionType;
}
