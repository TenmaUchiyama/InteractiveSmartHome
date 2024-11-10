import { edgeList, edgeStatus } from "@/store/flowStore";
import {
  SocketMessageType,
  type RunnningRoutineSocketData,
  type SocketMessageData,
} from "@/type/SocketMessageType";
import { get } from "svelte/store";
import FlowManager from "./FlowManager";

export default class SocketConnector {
  public static Instance: SocketConnector;

  private serverUrl: string = "ws://localhost:8080";
  private socket: WebSocket;

  constructor() {
    this.socket = new WebSocket(this.serverUrl);
  }
  init() {
    console.log("Initialize socket connection");
    this.socket.onopen = (server) => {
      // @ts-ignore
      console.log("Connected to server: ", server.currentTarget.url);
    };

    this.socket.onmessage = (message) => {
      console.log("Received message: ", message.data);
      const socketMsg = JSON.parse(message.data) as SocketMessageData;
      this.handleSocketMsg(socketMsg);
    };

    this.socket.onclose = () => {
      console.log("Disconnected from server");
    };

    this.socket.onerror = (error) => {
      console.log("Error", error);
    };
  }

  public static getInstance() {
    if (!SocketConnector.Instance) {
      SocketConnector.Instance = new SocketConnector();
    }
    return SocketConnector.Instance;
  }

  handleSocketMsg = (msg: SocketMessageData) => {
    switch (msg.message_type) {
      case SocketMessageType.Running_Routine:
        const runningRoutineMsg = msg as RunnningRoutineSocketData;
        const runningRoutineIds = runningRoutineMsg.running_routine.map(
          (r) => r.id
        );
        const mapEdgeIdFromRoutineId = runningRoutineIds.map((ids: string) =>
          FlowManager.getInstance().getEdgeIdFromRoutineId(ids)
        );
        edgeStatus.update((status) => {
          status.forEach((value, key) => {
            if (mapEdgeIdFromRoutineId.includes(key)) {
              status.set(key, true);
            } else {
              status.set(key, false);
            }
          });
          return status;
        });

        break;
    }
  };
}
