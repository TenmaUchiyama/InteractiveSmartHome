import { writable, type Writable } from "svelte/store";
import {
  ActionType,
  DeviceType,
  type IActionBlock,
  type IDBDeviceBlock,
  type IRoutineData,
  type ITimerLogicBlock,
} from "../type/ActionBlockInterface";
import { NodeType, type IDBNode } from "../type/NodeType";
import { type Node, type Edge } from "@xyflow/svelte";
import { DBConnector } from "@utils/DBConnector";
import { generateNode } from "@utils/utilFunctions";
export const test = () => {
  const timer: ITimerLogicBlock = {
    waitTime: 0,
    id: crypto.randomUUID(),
    name: "Timer Template",
    description: "This is a timer template",
    action_type: ActionType.Logic_Timer,
  };
};

export const initNodes = (): Node[] => {
  const node: Node[] = [];

  const timerNode: Node = generateNode(NodeType.Timer);

  const lightNode: Node = generateNode(NodeType.Light);
  const toggleButtonNode: Node = generateNode(NodeType.ToggleButton);

  node.push(timerNode);
  node.push(lightNode);
  node.push(toggleButtonNode);

  return node;
};

// export const initEdges = (): Edge[] => {
//   const edge : Edge[] = []

//   const edge1: Edge = {

//   }

// };

// Routineの追加。
// Actionの追加。
// Edge flow-edgeの追加。
// node flow-nodeの追加。
