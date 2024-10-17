import { castRunningRoutine } from "@/server/routes/routine/routineController";
import {
  SocketMessageData,
  RunnningRoutineSocketData,
  SocketMessageType,
} from "@/types/SocketMessageType";
import { WebSocket, WebSocketServer } from "ws";

export default class SocketBroadcaster {
  public static Instance: SocketBroadcaster;
  private port = 8080;
  private server: WebSocketServer;
  private clients: Set<WebSocket>;
  private msgHandlers: Array<(message: SocketMessageData) => void> = [];

  constructor() {
    this.server = new WebSocketServer({ port: this.port });
    this.clients = new Set<WebSocket>();

    console.log(`Socket server started on port ${this.port}`);
    this.server.on("connection", (ws) => {
      console.log("[SOCKET BROADCASTER] Client has connected");
      this.clients.add(ws);
      castRunningRoutine();
    });
  }

  public static getInstance() {
    if (!SocketBroadcaster.Instance) {
      SocketBroadcaster.Instance = new SocketBroadcaster();
    }
    return SocketBroadcaster.Instance;
  }

  registerCallback(callback: (message: SocketMessageData) => void) {}

  async broadcastMessage(message: SocketMessageData) {
    const body = JSON.stringify(message);
    console.log("[SOCKET BROADCASTER] Broadcasting message");
    this.clients.forEach(async (client) => {
      if (client.readyState === WebSocket.OPEN) {
        await client.send(body);
      }
    });
  }
}
