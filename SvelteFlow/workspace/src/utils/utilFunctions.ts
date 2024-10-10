import type { Edge, Node } from "@xyflow/svelte";
import {
  type IRoutineData,
  type IRoutine,
  ActionType,
  DeviceType,
  type IDBDeviceBlock,
  type ITimerLogicBlock,
} from "@type/ActionBlockInterface";

import TimerLogicNode from "@nodes/logic/TimerLogicNode.svelte";
import ToggleBtnSensorNode from "@nodes/device/sensor/ToggleBtnSensorNode.svelte";
import LightActuatorNode from "@nodes/device/actuator/LightActuatorNode.svelte";
import {
  NodeType,
  type IDBEdge,
  type IDBNode,
  type IEdge,
} from "@type/NodeType";
import { DBConnector } from "./DBConnector";

const genNodeDataForDB = (node: Node[]): IDBNode[] => {
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

export const extractEdge = (edges: IDBEdge): Edge[] => {
  return edges.edges.map((edge) => {
    return {
      id: edge.id,
      source: edge.source,
      target: edge.target,
    };
  });
};
export async function getEdge(edge_id: string): Promise<IDBEdge | null> {
  const edgeData = await DBConnector.Instance.getEdge(edge_id);
  return edgeData;
}

export async function getNodes(node_id: string[]) {
  const nodeData = await DBConnector.Instance.getNode(node_id);
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

export function convertEdgesToRoutines(
  edges: IEdge[],
  nodes: Node[]
): IRoutine[] {
  // ノードIDからaction_data.idへのマッピングを作成
  const nodeIdToActionId = new Map<string, string>();
  nodes.forEach((node) => {
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

export const sendWholeData = async (nodes: Node[], edges: Edge[]) => {
  const actions = nodes.map((node) => node.data.action_data);
  // @ts-ignore
  let nodesDBData: IDBNode[] = nodes.map((node) => {
    return {
      id: node.id,
      type: node.type,
      // @ts-ignore
      data_action_id: node.data.action_data.id,
      position: node.position,
    };
  });

  const edgeDataArray: IEdge[] = edges.map((edge) => {
    return {
      id: edge.id,
      source: edge.source,
      target: edge.target,
    };
  });

  const routine: IRoutineData = convertEdgesToRoutines(edges, nodes);

  const dbEdge: IDBEdge = {
    id: crypto.randomUUID(),
    associated_routine_id: routine.id,
    edges: edgeDataArray,
  };

  await DBConnector.Instance.addRoutine(routine);
  await DBConnector.Instance.addAction(actions);
  await DBConnector.Instance.addNode(nodesDBData);
  await DBConnector.Instance.addEdge(dbEdge);

  // await DBConnector.Instance.addRoutine(routine);
  // await DBConnector.Instance.addNodes(nodesDBData);
};

export const startRoutine = async (nodes: Node[], edges: Edge[]) => {
  const nodesDBData: IDBNode[] = genNodeDataForDB(nodes);
  await DBConnector.Instance.updateNode(nodesDBData);
  // edgeの更新
  const edgeData = await getEdge("fb91ff72-0c63-4ac5-8e69-42f480d6f872");
  await DBConnector.Instance.updateEdge(edgeData!);
  //routineの更新
  const routine = convertEdgesToRoutines(edges, nodes);
  console.log("ROUTINE: ", routine);
  const getOriginalRoutine = await DBConnector.Instance.getRoutine(
    edgeData!.associated_routine_id
  );
  if (!getOriginalRoutine) return;
  getOriginalRoutine.action_routine = routine;

  await DBConnector.Instance.updateRoutine(getOriginalRoutine);
  // actionの更新
  const action = nodes.map((node: Node) => node.data.action_data);
  await DBConnector.Instance.updateAction(action);
  // routineを開始させる
  const routineId = edgeData!.associated_routine_id;
  await DBConnector.Instance.startRoutine(routineId);
};

export const getFlowData = async (
  edgeId: string
): Promise<{ nodes: Node[] | null; edges: IDBEdge | null }> => {
  const edges = await getEdge("fb91ff72-0c63-4ac5-8e69-42f480d6f872");
  if (!edges) return { nodes: [], edges: null };
  const uniqueNodes = new Set<string>();

  edges.edges.forEach((edge: any) => {
    uniqueNodes.add(edge.source);
    uniqueNodes.add(edge.target);
  });
  const uniqueNodesArray = Array.from(uniqueNodes);
  const nodes = await getNodes(uniqueNodesArray);

  return { nodes, edges };
};
