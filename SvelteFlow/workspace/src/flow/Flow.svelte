<script lang="ts">
  import {
    SvelteFlow,
    Controls,
    Background,
    MiniMap,
    type Node,
    useSvelteFlow,
  } from "@xyflow/svelte";
  import SideBar from "@/components/SideBar.svelte";
  import { nodes, edges, useDnD } from "@/store/flowStore";
  import {
    extractEdge,
    getFlowData,
    startRoutine,
  } from "@/utils/utilFunctions";
  import { NodeType, type IEdge } from "@type/NodeType";
  import { onMount } from "svelte";
  import { generateNode } from "@/utils/NodeGenerator";
  import StartRoutineButton from "@/components/StartRoutineButton.svelte";
  import type { IRoutineData } from "@/type/ActionBlockInterface";
  import SimpleComparatorLogicNode from "@/nodes/logic/SimpleComparatorLogicNode.svelte";
  import TimerLogicNode from "@/nodes/logic/TimerLogicNode.svelte";
  import { nodeTypes } from "@/type/NodeTypeMaps";
  const type = useDnD();
  const { screenToFlowPosition } = useSvelteFlow();
  const onStartRoutine = async () => {
    console.log($nodes);
    console.log($edges);
    await startRoutine($nodes, $edges);
  };

  onMount(async () => {
    const flowData = await getFlowData("fb91ff72-0c63-4ac5-8e69-42f480d6f872");
    console.log(flowData.edges);
    if (flowData.nodes) $nodes = flowData.nodes;
    if (flowData.edges) $edges = extractEdge(flowData.edges);
  });

  const onDragOver = (event: DragEvent) => {
    event.preventDefault();

    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = "move";
    }
  };

  const onDrop = (event: DragEvent) => {
    event.preventDefault();

    if (!$type) {
      return;
    }
    const position = screenToFlowPosition({
      x: event.clientX,
      y: event.clientY,
    });

    const newNode: Node = generateNode($type as NodeType);
    newNode.position = position;

    $nodes = [...$nodes, newNode];
  };
</script>

<SvelteFlow
  {nodes}
  {edges}
  {nodeTypes}
  fitView
  on:dragover={onDragOver}
  on:drop={onDrop}
>
  <StartRoutineButton on:start-routine={onStartRoutine} />

  <Controls />
  <Background />
  <MiniMap />
  <SideBar />
</SvelteFlow>
