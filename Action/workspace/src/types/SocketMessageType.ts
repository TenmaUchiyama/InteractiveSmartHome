import { IRoutineData } from "./ActionBlockInterfaces";

export enum SocketMessageType {
  Running_Routine = "running_routine",
  Stopping_Routine = "routine_stopped",
}

export type SocketMessageData = {
  message_type: SocketMessageType;
};

export type RunnningRoutineSocketData = SocketMessageData & {
  running_routine: IRoutineData[];
};

export type StoppingRoutineSocketData = SocketMessageData & {
  routine_id: IRoutineData[];
};
