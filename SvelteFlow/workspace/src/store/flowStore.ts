import { writable, get, type Writable, derived } from "svelte/store";
import { type Node, type Edge } from "@xyflow/svelte";
import { getContext } from "svelte";
import type { RoutineEdge, IDBNode, IEdge } from "@/type/NodeType";
import FlowStoreManager from "@/utils/FlowManager";
import FlowManager from "@/utils/FlowManager";

export const nodes = writable<Node[]>([]);

export const edges = writable<Edge[]>([]);

export const nodeList = writable<Node[]>([]);

export const edgeList = writable<RoutineEdge[]>([]);

export const selectedNodes = writable<string[]>([]);

export const selectedEdge = writable<string | null>(null);

export const selectedEdgeIndex = writable<number>(0);

export const edgeStatus = writable<Map<string, boolean>>(new Map());

export const rightClicked = writable<boolean>(false);
export const useDnD = () => {
  return getContext("dnd") as Writable<string | null>;
};
