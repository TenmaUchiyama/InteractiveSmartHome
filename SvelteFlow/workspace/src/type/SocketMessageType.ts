import type { IRoutineData } from "./ActionBlockInterface";

export enum SocketMessageType {
  Running_Routine = "running_routine",
}

export type SocketMessageData = {
  message_type: SocketMessageType;
};

export type RunnningRoutineSocketData = SocketMessageData & {
  running_routine: IRoutineData[];
};
