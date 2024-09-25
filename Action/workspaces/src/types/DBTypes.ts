export type RoutineData = {
  id: string;
  name: string;
  actionBlocks: ActionSequence[];
};

export type ActionSequence = {
  first?: boolean;
  actionId: string;
  nextActionId: string | string[] | null;
};

export type DeviceData = {
  id: string;
  topic: string;
  name: string;
  deviceType: DeviceType;
  valueType: 'binary' | 'value';
  location: {
    x: number;
    y: number;
    z: number;
  };
};
