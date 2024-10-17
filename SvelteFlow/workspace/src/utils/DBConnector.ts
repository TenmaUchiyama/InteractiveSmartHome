import type { IDeviceData, IRoutineData } from "@type/ActionBlockInterface";
import type { RoutineEdge, IDBNode } from "@type/NodeType";

export class DBConnector {
  private baseUrl = "http://localhost:4049";
  private deviceUrl = this.baseUrl + "/device";
  private actionUrl = this.baseUrl + "/action";
  private routineUrl = this.baseUrl + "/routine";
  private nodeUrl = this.baseUrl + "/flow-node";
  private edgeUrl = this.baseUrl + "/flow-edge";

  public static Instance: DBConnector = new DBConnector();

  public static getInstance() {
    if (this.Instance == null) {
      this.Instance = new DBConnector();
    }
    return this.Instance;
  }

  private async handleResult(result: Response) {
    if (result.ok) {
      // JSONパースが可能かどうかを確認する
      const contentType = result.headers.get("content-type");
      if (contentType && contentType.includes("application/json")) {
        const data = await result.json();
      } else {
        // JSONでない場合はテキストとして処理
        const text = await result.text();
      }
    } else {
      console.error("Error: ", result.statusText);
    }
  }

  async connectionTest() {
    const result = await fetch(this.baseUrl);
    this.handleResult(result);
  }

  async getDevice(device_id: string): Promise<IDeviceData | null> {
    const url = this.deviceUrl + "/get/" + device_id;
    const result = await fetch(url, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (result.ok) {
      const data = await result.json();
      return data;
    }

    return null;
  }

  async startRoutine(routine_id: string) {
    const url = this.routineUrl + "/start/" + routine_id;
    const result = await fetch(url, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    });

    this.handleResult(result);
  }

  async stopRoutine(routine_id: string) {
    const url = this.routineUrl + "/stop/" + routine_id;
    const result = await fetch(url, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    });

    this.handleResult(result);
  }

  async startAllRoutine() {
    const url = this.routineUrl + "/start-all";
    const result = await fetch(url, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    });

    this.handleResult(result);
  }

  async stopAllRoutine() {
    const url = this.routineUrl + "/stop-all";
    const result = await fetch(url, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    });

    this.handleResult(result);
  }
  async restartRoutine(routine_id: string) {
    const url = this.routineUrl + "/restart/" + routine_id;
    const result = await fetch(url, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    });

    this.handleResult(result);
  }
  async getRoutine(routine_id: string): Promise<IRoutineData | null> {
    const url = this.routineUrl + "/get/" + routine_id;
    const result = await fetch(url, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (result.ok) {
      const data = await result.json();
      return data;
    }

    return null;
  }

  async addRoutine(routine: IRoutineData) {
    const url = this.routineUrl + "/add";

    const result = await fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(routine),
    });

    this.handleResult(result);
  }

  async updateRoutine(routine: IRoutineData) {
    const url = this.routineUrl + "/update";
    const result = await fetch(url, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(routine),
    });

    this.handleResult(result);
  }

  async deleteRoutine(routine_id: string) {
    const url = this.routineUrl + "/delete/" + routine_id;
    const result = await fetch(url, {
      method: "DELETE",
    });

    this.handleResult(result);
  }

  async getAction(action_id: string) {
    const url = this.actionUrl + "/get/" + action_id;
    try {
      const result = await fetch(url, {
        method: "GET",
        headers: {
          "Content-Type": "application/json",
        },
      });

      if (result.ok) {
        const data = await result.json();
        return data;
      }
    } catch (e) {
      console.error(e);
    }
  }

  async addAction(action: any) {
    const url = this.actionUrl + "/add";

    const result = await fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(action),
    });

    this.handleResult(result);
  }

  async updateAction(action: any[]) {
    const url = this.actionUrl + "/update";
    const result = await fetch(url, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(action),
    });
    this.handleResult(result);
  }

  async deleteActions(action_ids: string[]) {
    const url = this.actionUrl + "/delete";
    const body = {
      ids: action_ids,
    };
    const result = await fetch(url, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(body),
    });

    this.handleResult(result);
  }

  async getAllNode(): Promise<IDBNode[]> {
    const url = this.nodeUrl + "/get-all";
    const result = await fetch(url, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (result.ok) {
      const data = await result.json();
      return data;
    } else {
      console.error("Error: ", result.statusText);
      return [];
    }
  }
  async getNode(node_id: string | string[]) {
    let url = "";
    let config = {};
    if (Array.isArray(node_id)) {
      url = this.nodeUrl + "/get/";
      config = {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(node_id),
      };
    } else {
      url = this.nodeUrl + "/get/" + node_id;
      config = { method: "GET" };
    }

    const result = await fetch(url, config);

    if (result.ok) {
      const data = await result.json();
      return data;
    }
  }

  async addNode(node: IDBNode | IDBNode[]) {
    const url = this.nodeUrl + "/add";
    const result = await fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(node),
    });

    this.handleResult(result);
  }

  async updateNode(node: IDBNode[]) {
    const url = this.nodeUrl + "/update";
    const result = await fetch(url, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(node),
    });

    this.handleResult(result);
  }

  async getEdge(edge_id: string): Promise<RoutineEdge | null> {
    const url = this.edgeUrl + "/get/" + edge_id;
    const result = await fetch(url, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (result.ok) {
      const data = (await result.json()) as RoutineEdge;
      return data;
    }
    return null;
  }

  async getAllEdge(): Promise<RoutineEdge[]> {
    const url = this.edgeUrl + "/get-all";
    const result = await fetch(url, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (result.ok) {
      const data = await result.json();
      return data;
    } else {
      console.error("Error: ", result.statusText);
      return [];
    }
  }

  async addEdge(edge: RoutineEdge) {
    const url = this.edgeUrl + "/add";
    const result = await fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(edge),
    });

    this.handleResult(result);
  }

  async deleteNode(id: string) {
    const url = this.nodeUrl + "/delete/" + id;
    const result = await fetch(url, {
      method: "DELETE",
    });

    this.handleResult(result);
  }

  async deleteNodes(ids: string[]) {
    const url = this.nodeUrl + "/delete";
    const body = {
      ids: ids,
    };
    const result = await fetch(url, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(body),
    });

    this.handleResult(result);
  }

  async deleteEdge(id: string) {
    const url = this.edgeUrl + "/delete/" + id;
    const result = await fetch(url, {
      method: "DELETE",
    });

    this.handleResult(result);
  }

  async updateEdge(edge: RoutineEdge) {
    const url = this.edgeUrl + "/update";
    const result = await fetch(url, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(edge),
    });

    this.handleResult(result);
  }
}
