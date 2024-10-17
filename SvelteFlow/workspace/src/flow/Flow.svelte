<script lang="ts">
  import {
    SvelteFlow,
    Controls,
    Background,
    MiniMap,
    type Node,
    useSvelteFlow,
  } from "@xyflow/svelte";
  import SideBar from "@/components/sidebar/SideBar.svelte";
  import {
    nodes,
    edges,
    useDnD,
    selectedEdge,
    rightClicked,
  } from "@/store/flowStore";
  import { NodeType, type IEdge } from "@type/NodeType";
  import { onMount } from "svelte";
  import { generateNode } from "@/utils/NodeGenerator";
  import StartRoutineButton from "@/components/RoutineController.svelte";
  import { nodeTypes } from "@/type/NodeTypeMaps";
  import FlowManager from "@/utils/FlowManager";
  import RightClickMenu from "@/components/RightClickMenu.svelte";
  import { writable } from "svelte/store";
  import GlobalContextMenu from "@/components/GlobalContextMenu.svelte";
  import { contextMenu, type MenuItem } from "@/store/contextMenuStore";
  let selectedNode: Node | null = null;
  const type = useDnD();
  const { screenToFlowPosition } = useSvelteFlow();

  const onStartRoutine = async () => {
    let currentSelectedEdge = $selectedEdge;
    if (!currentSelectedEdge) return;
    await FlowManager.getInstance().startRoutine(currentSelectedEdge);
  };

  const onStopRoutine = async () => {
    let currentSelectedEdge = $selectedEdge;
    if (!currentSelectedEdge) return;
    await FlowManager.getInstance().stopRoutine(currentSelectedEdge);
  };

  onMount(async () => {
    window.addEventListener("keydown", handleKeydown);
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
    FlowManager.getInstance().addNode(newNode, $selectedEdge!);

    $type = null;
  };

  async function handleKeydown(event: KeyboardEvent) {
    if (event.ctrlKey && event.key === "s") {
      event.preventDefault(); // ブラウザのデフォルト動作を防ぐ (保存ダイアログを防ぐ)
      if ($selectedEdge) {
        await FlowManager.getInstance().updateFlowDB($selectedEdge!);

        console.log("saved");
      }
    }
  }

  function handleContextMenu(event: CustomEvent) {
    event.preventDefault();
    let mouseEvent = event.detail.event;
    mouseEvent.preventDefault();

    let selectedNode = event.detail.node;
    let nodeMenuItem: MenuItem[] = [
      {
        label: "Duplicate",
        color: "black",
        action: () => {
          if (selectedNode === null) return;
          FlowManager.getInstance().duplicateNode(selectedNode, $selectedEdge!);
        },
      },

      {
        label: "Delete",
        color: "red",
        action: () => {
          if (selectedNode === null) return;
          $nodes = $nodes.filter((n) => n.id !== selectedNode!.id);

          if ($selectedEdge) {
            FlowManager.getInstance().updateFlowDB($selectedEdge);

            FlowManager.getInstance().deleteNode([selectedNode.id]);
          }
        },
      },
    ];

    let position = { x: mouseEvent.x, y: mouseEvent.y };

    contextMenu.set({ menuItems: nodeMenuItem, position });
  }

  let addSvelteFowNodes = writable<any>();
  $: console.log($addSvelteFowNodes);
</script>

<SvelteFlow
  {nodes}
  {edges}
  {nodeTypes}
  addSelectedNodes={addSvelteFowNodes}
  deleteKey={["Delete", "Backspace"]}
  fitView
  ondelete={(data) => {
    if ($selectedEdge) {
      FlowManager.getInstance().updateFlowDB($selectedEdge);

      if (data.nodes) {
        FlowManager.getInstance().deleteNode(data.nodes.map((node) => node.id));
      }
    }
  }}
  multiSelectionKey="ctrlKey"
  on:nodecontextmenu={handleContextMenu}
  on:dragover={onDragOver}
  on:drop={onDrop}
>
  <StartRoutineButton
    on:start-routine={onStartRoutine}
    on:stop-routine={onStopRoutine}
  />

  <Controls />
  <Background />
  <MiniMap />
  <SideBar />
</SvelteFlow>
<!-- <RightClickMenu menuItems={nodeMenuItem} position={contextPosition} /> -->
<GlobalContextMenu />
