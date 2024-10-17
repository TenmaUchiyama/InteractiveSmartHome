import {
  nodes,
  edges,
  nodeList,
  edgeList,
  selectedEdge,
  selectedEdgeIndex,
  edgeStatus,
} from "@/store/flowStore";
import type { Edge, Node } from "@xyflow/svelte";
import type { RoutineEdge, IDBNode, IEdge } from "@/type/NodeType";
import { DBConnector } from "./DBConnector";
import { get } from "svelte/store";
import type { IRoutine, IRoutineData } from "@/type/ActionBlockInterface";

export const handleStyle = "width: 20px; height: 20px;";

export default class FlowManager {
  public static Instance: FlowManager = new FlowManager();

  public static getInstance() {
    if (this.Instance == null) {
      this.Instance = new FlowManager();
    }
    return this.Instance;
  }

  async initStore() {
    const nodeDatas = await this.getNodes();

    nodeList.set(nodeDatas);

    const edgeDatas = await DBConnector.getInstance().getAllEdge();
    edgeStatus.update((status) => {
      edgeDatas.forEach((edge) => {
        status.set(edge.id, false);
      });
      return status;
    });
    edgeList.set(edgeDatas);
  }

  async initScene() {
    const allEdge = get(edgeList);
    if (allEdge.length === 0) return;
    const firstEdge = allEdge[0];
    selectedEdge.set(firstEdge.id);
    const { nodeInEdge, edgeData } = await this.getFlowData(firstEdge);

    nodes.set(nodeInEdge);
    if (edgeData) edges.set(this.extractEdge(edgeData));
  }

  extractEdge = (edges: RoutineEdge): Edge[] => {
    return edges.edges.map((edge) => {
      return {
        id: edge.id,
        source: edge.source,
        target: edge.target,
      };
    });
  };

  async getNodes(node_ids?: string[]): Promise<Node[]> {
    let nodeData: IDBNode[] = [];
    if (!node_ids) {
      nodeData = await DBConnector.getInstance().getAllNode();
    } else {
      if (node_ids.length === 0) return [];
      nodeData = await DBConnector.getInstance().getNode(node_ids);
    }

    if (!nodeData) return [];
    const nodeActionData = await Promise.all(
      nodeData.map(async (node: any) => {
        const actionData = await DBConnector.Instance.getAction(
          node.data_action_id
        );

        return { ...node, data: { action_data: actionData } };
      })
    );

    return nodeActionData;
  }

  getDBEdgeFromId(edge_id: string): RoutineEdge | null {
    const edgeData = get(edgeList).find((edge) => edge.id === edge_id);
    return edgeData || null;
  }

  getEdgeIdFromRoutineId(routine_id: string): string | undefined {
    const edgeData = get(edgeList).find(
      (edge) => edge.associated_routine_id === routine_id
    );

    return edgeData?.id;
  }

  /**
   * ノードリストの更新
   * 現在のエッジにノードを追加する。
   * Actionの追加。
   * DBの更新
   */
  async addNode(node: Node, edge_id: string) {
    nodeList.set([...get(nodeList), node]);
    nodes.update((nodes) => [...nodes, node]);
    edgeList.update((edges) => {
      return edges.map((edge) => {
        if (edge.id === edge_id) {
          edge.nodes.push(node.id);
        }
        return edge;
      });
    });

    const getEdge = get(edgeList).find((edge) => edge.id === edge_id);
    if (!getEdge) {
      console.error("Edge not found");
      return;
    }

    const data = this.genNodeDataForDB([node]);
    await DBConnector.getInstance().addNode(data);
    await DBConnector.getInstance().addAction(node.data.action_data);
    await DBConnector.getInstance().updateEdge(getEdge);
  }

  async duplicateNode(node: Node, edge_id: string) {
    console.log(node);
    // avoiding reference issue
    let newNode = JSON.parse(JSON.stringify(node));
    newNode.id = crypto.randomUUID();
    newNode.data.action_data.id = crypto.randomUUID();
    let currPosition = newNode.position;
    newNode.position = { x: currPosition.x + 40, y: currPosition.y + 40 };

    this.addNode(newNode, edge_id);
  }

  async setSelectEdge(edge_id: string | null) {
    const selectedEdgeId = get(selectedEdge);
    if (selectedEdgeId) {
      this.updateFlowList(selectedEdgeId);
    }

    selectedEdge.set(edge_id);

    if (edge_id === null) return;

    const { nodeInEdge, edgeData } = await this.getFlowData(edge_id);

    nodes.set(nodeInEdge);

    if (!edgeData) return;
    const edge = this.extractEdge(edgeData);
    edges.set(edge);
  }
  updateNodeList() {
    const currentNodes = get(nodes);
    const nodeLists = get(nodeList);

    for (let node of currentNodes) {
      const existingIndex = nodeLists.findIndex((n) => n.id === node.id);

      if (existingIndex === -1) {
        nodeLists.push(node);
      } else {
        nodeLists[existingIndex] = { ...nodeLists[existingIndex], ...node };
      }
    }

    nodeList.set(nodeLists);
  }
  updateFlowList(update_edge_id: string) {
    this.updateNodeList();
    this.updateEdgeList(update_edge_id);
  }

  async getFlowData(
    edge_data: string | RoutineEdge
  ): Promise<{ nodeInEdge: Node[]; edgeData: RoutineEdge | null }> {
    let edgeData: RoutineEdge | undefined;
    if (typeof edge_data === "string") {
      edgeData = get(edgeList).find((edge) => edge.id === edge_data);
    } else {
      edgeData = edge_data;
    }
    if (!edgeData) return { nodeInEdge: [], edgeData: null };
    const uniqueIds = edgeData.nodes;

    const nodesData = await this.getNodes(uniqueIds);

    return { nodeInEdge: nodesData, edgeData };
  }

  async createNewFlow() {
    let routine: IRoutineData = {
      id: crypto.randomUUID(),
      name: "Undefined",
      action_routine: [],
    };
    let newEdge: RoutineEdge = {
      id: crypto.randomUUID(),
      associated_routine_id: routine.id,
      routine_name: routine.name,
      nodes: [],
      edges: [],
    };

    edgeList.update((edges) => {
      edges.push(newEdge);
      return edges;
    });

    let currentEdgeId = get(selectedEdge);
    if (currentEdgeId) {
      this.updateFlowList(currentEdgeId);
    }

    nodes.set([]);
    edges.set([]);

    selectedEdge.set(newEdge.id);
    edgeStatus.update((status) => {
      status.set(newEdge.id, false);
      return status;
    });
    await DBConnector.getInstance().addRoutine(routine);
    await DBConnector.getInstance().addEdge(newEdge);
  }
  async updateFlowDB(edge_id: string) {
    let currentNode = get(nodes);

    const nodesDBData: IDBNode[] = this.genNodeDataForDB(currentNode);
    await DBConnector.getInstance().updateNode(nodesDBData);

    // actionの更新
    const actions = currentNode.map((node: Node) => node.data.action_data);
    await DBConnector.getInstance().updateAction(actions);

    let currentEdges = get(edges);

    const edgeData = this.getDBEdgeFromId(edge_id);
    if (!edgeData) return;

    edgeData.edges = currentEdges;
    edgeData.nodes = currentNode.map((node) => node.id);
    await DBConnector.getInstance().updateEdge(edgeData);

    //routineの更新
    let edge: IEdge[] = edgeData.edges;
    const routine = this.convertEdgesToRoutines(edge, currentNode);

    const getOriginalRoutine = await DBConnector.getInstance().getRoutine(
      edgeData.associated_routine_id
    );
    if (!getOriginalRoutine) return;
    getOriginalRoutine.action_routine = routine;

    await DBConnector.getInstance().updateRoutine(getOriginalRoutine);
  }

  updateEdgeList(update_edge_id: string) {
    const edgeData = get(edges);
    const edgeLists = get(edgeList);

    const updatingEdge = edgeLists.find((edge) => edge.id === update_edge_id);
    if (!updatingEdge) return;

    updatingEdge.edges = edgeData;
  }

  updateEdgeDb(edge: RoutineEdge) {
    const edgeLists = get(edgeList);

    const updatedEdgeLists = edgeLists.map((e) =>
      e.id === edge.id ? edge : e
    );
    edgeList.set(updatedEdgeLists);
  }

  genNodeDataForDB = (node: Node[]): IDBNode[] => {
    return node.map((node) => {
      return {
        id: node.id,
        type: node.type,
        // @ts-ignore
        data_action_id: node.data.action_data.id,
        position: node.position,
      };
    }) as IDBNode[];
  };

  convertEdgesToRoutines(edges: IEdge[], nodes: Node[]): IRoutine[] {
    // ノードIDからaction_data.idへのマッピングを作成
    const nodeIdToActionId = new Map<string, string>();
    nodes.forEach((node) => {
      // @ts-ignore
      nodeIdToActionId.set(node.id, node.data.action_data.id);
    });

    // ソースIDとターゲットIDのセットを作成
    const sourceIDs = edges.map((edge) => edge.source);
    const targetIDs = edges.map((edge) => edge.target);
    const sourceIDSet = new Set(sourceIDs);
    const targetIDSet = new Set(targetIDs);

    // 最初のノードと最後のノードを特定
    const firstNodeIDs = [...sourceIDSet].filter((id) => !targetIDSet.has(id));
    const lastNodeIDs = [...targetIDSet].filter((id) => !sourceIDSet.has(id));

    // エッジをIRoutineに変換
    const routines: IRoutine[] = edges.map((edge) => {
      const currentActionId = nodeIdToActionId.get(edge.source) || "";
      const nextActionId = nodeIdToActionId.get(edge.target) || "";

      const routine: IRoutine = {
        current_block_id: currentActionId,
        next_block_id: nextActionId,
      };

      if (firstNodeIDs.includes(edge.source)) {
        routine.first = true;
      }

      return routine;
    });

    // 最後のノードに対するルーチンを追加
    lastNodeIDs.forEach((lastNodeId) => {
      const currentActionId = nodeIdToActionId.get(lastNodeId) || "";

      // 既存のルーチンでcurrent_block_idが同じものがないか確認
      const existingRoutine = routines.find(
        (routine) =>
          routine.current_block_id === currentActionId &&
          routine.next_block_id === ""
      );

      if (!existingRoutine) {
        // 新しいルーチンを追加
        routines.push({
          current_block_id: currentActionId,
          next_block_id: "",
          last: true,
        });
      } else {
        // 既存のルーチンにlastフラグを追加
        existingRoutine.last = true;
      }
    });

    return routines;
  }

  /**
   * 現在のEdgeのデータをRoutineにして実行する
   *
   * 1. 現在のNodeのデータをDBに保存
   * 2. 現在のEdgeのデータをDBに保存
   * 3. EdgeのデータをRoutineに変換
   * 4. RoutineのデータをDBに保存
   * 5. ActionのデータをDBに保存
   * 6. Routineを開始
   *
   *
   *
   * @param edge_id
   * @returns
   */
  startRoutine = async (edge_id: string) => {
    await this.updateFlowDB(edge_id);

    const edgeData = this.getDBEdgeFromId(edge_id);
    if (!edgeData) return;
    const routineId = edgeData.associated_routine_id;
    await DBConnector.getInstance().startRoutine(routineId);
  };

  stopRoutine = async (edge_id: string) => {
    const edgeData = this.getDBEdgeFromId(edge_id);
    if (!edgeData) return;
    const routineId = edgeData.associated_routine_id;
    await DBConnector.getInstance().stopRoutine(routineId);
  };

  deleteEdge = async (edge_id: string) => {
    edgeList.update((edges) => {
      const newEdge = edges.filter((edge) => edge.id !== edge_id);
      const deleteEdge = edges.find((edge) => edge.id === edge_id);
      if (deleteEdge) {
        const routineId = deleteEdge.associated_routine_id;

        edgeStatus.update((status) => {
          status.delete(routineId);
          return status;
        });
      }

      return newEdge;
    });

    await DBConnector.getInstance().deleteEdge(edge_id);
  };

  /**
   * node_idからnodeを削除する
   * nodeListから除く
   * nodesから除く
   *
   * nodeDbから削除
   * actionDbから削除
   * @param node_ids
   */
  deleteNode = async (node_ids: string[]) => {
    await DBConnector.getInstance().deleteNodes(node_ids);

    const deletingActionIds = get(nodeList)
      .filter((node) => node_ids.includes(node.id))
      // @ts-ignore
      .map((node) => node.data.action_data.id);

    // actionの削除
    await DBConnector.getInstance().deleteActions(deletingActionIds);
    nodeList.update((nodes) =>
      nodes.filter((node) => !node_ids.includes(node.id))
    );
    // nodeListから削除
    nodeList.update((nodes) =>
      nodes.filter((node) => !node_ids.includes(node.id))
    );
  };

  deleteFlow = async (edge_id: string) => {
    let edgesData = get(edgeList);
    // edgeの中のnode、actionを削除
    const deleteEdge = edgesData.find((edge) => edge.id === edge_id);
    if (!deleteEdge) return;

    // edgeの削除
    this.deleteEdge(edge_id);
    // edgeのroutine削除
    await DBConnector.getInstance().deleteRoutine(
      deleteEdge.associated_routine_id
    );

    const currentNodes = get(nodes);
    const uniqueIds = currentNodes.map((node) => node.id);

    // nodeの削除F
    this.deleteNode(uniqueIds);

    selectedEdgeIndex.update((ind: number) => {
      const getEdges = get(edgeList);
      if (getEdges.length === 0) {
        return 0;
      }
      let edgeListLength = getEdges.length;
      let newInd = (ind - 1 + edgeListLength) % edgeListLength;

      let getEdgeId = getEdges[newInd].id;

      FlowManager.getInstance().setSelectEdge(getEdgeId);
      return newInd;
    });
  };
}
