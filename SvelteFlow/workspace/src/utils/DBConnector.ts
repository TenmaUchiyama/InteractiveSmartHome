import type { IDeviceData, IRoutineData } from "@type/ActionBlockInterface";
import type { IDBEdge, IDBNode } from "@type/NodeType";

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
        console.log("JSON Response: ", data);
      } else {
        // JSONでない場合はテキストとして処理
        const text = await result.text();
        console.log("Text Response: ", text);
      }
    } else {
      console.error("Error: ", result.statusText);
    }
  }

  async connectionTest() {
    const result = await fetch(this.baseUrl);
    this.handleResult(result);
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

  async getEdge(edge_id: string): Promise<IDBEdge | null> {
    const url = this.edgeUrl + "/get/" + edge_id;
    const result = await fetch(url, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (result.ok) {
      const data = (await result.json()) as IDBEdge;
      return data;
    }
    return null;
  }

  async getAllEdge(): Promise<IDBEdge[]> {
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

  async addEdge(edge: IDBEdge) {
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

  async updateEdge(edge: IDBEdge) {
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
