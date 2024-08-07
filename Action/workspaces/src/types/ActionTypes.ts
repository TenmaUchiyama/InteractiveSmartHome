export interface IRoutine {
  id: string;
  name: string;
  actionBlocks: IActionSequence[];
}

export interface IActionSequence {
  first?: boolean;
  actionId: string;
  nextActionId: string;
}

export interface IAction {
  id: string;
  actionType: string;
  deviceId?: string;
  name: string;
  description: string;
}

export interface IDevice {
  id: string;
  topic: string;
  name: string;
  deviceType: string;
  valueType: string;
  location: {
    x: number;
    y: number;
    z: number;
  };
}
