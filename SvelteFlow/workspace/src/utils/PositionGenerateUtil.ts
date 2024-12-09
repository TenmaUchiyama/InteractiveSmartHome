import type { IEdge } from "@/type/NodeType";
import dagre from "dagre";
import type { Node } from "@xyflow/svelte";
export function addPositionsToNodes(nodes: Node[], edges: IEdge[]): Node[] {
  const g = new dagre.graphlib.Graph();
  g.setGraph({
    rankdir: "LR",
    ranksep: 100,
    nodesep: 150,
  });
  g.setDefaultEdgeLabel(() => ({}));

  nodes.forEach((node) => {
    g.setNode(node.id, {
      label: node.data.action_data,
      width: 200,
      height: 250,
    });
  });

  edges.forEach((edge) => {
    g.setEdge(edge.source, edge.target);
  });

  dagre.layout(g);

  return nodes.map((node) => {
    const { x, y } = g.node(node.id);
    return {
      ...node,
      position: { x, y },
    };
  });
}
