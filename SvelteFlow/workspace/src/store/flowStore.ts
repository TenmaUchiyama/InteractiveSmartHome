import { writable, type Writable } from "svelte/store";
import { type Node, type Edge } from "@xyflow/svelte";
import { getContext } from "svelte";

export const nodes = writable<Node[]>([]);

export const edges = writable<Edge[]>([]);

export const selectedEdge = writable<Edge | null>(null);

export const selectedNode = writable<Node | null>(null);

export const useDnD = () => {
  return getContext("dnd") as Writable<string | null>;
};
