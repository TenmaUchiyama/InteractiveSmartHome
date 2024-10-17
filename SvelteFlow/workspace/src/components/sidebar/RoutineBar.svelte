<script lang="ts">
  import type { RoutineEdge } from "@type/NodeType";

  import {
    edgeList,
    rightClicked,
    edgeStatus,
    selectedEdge,
  } from "@/store/flowStore";
  import { DBConnector } from "@/utils/DBConnector";
  import RightClickMenu from "../RightClickMenu.svelte";
  import { onMount } from "svelte";
  import FlowManager from "@/utils/FlowManager";
  import { contextMenu, type MenuItem } from "@/store/contextMenuStore";

  let menuItems: { label: string; color: string; action: () => void }[] = [];
  let position = { x: 0, y: 0 };
  let visible = false;

  let editableEdge: RoutineEdge | null = null; // 編集対象のエッジ
  let newRoutineName = ""; // 新しいルーチン名

  onMount(async () => {});

  function handleContextMenu(event: MouseEvent, edge: RoutineEdge) {
    event.preventDefault();
    const position = { x: event.clientX, y: event.clientY };

    const routineMenu: MenuItem = $edgeStatus.get(edge.id)
      ? {
          label: `Stop`,
          color: "black",
          action: async () => {
            await FlowManager.getInstance().stopRoutine(edge.id);
          },
        }
      : {
          label: `Start`,
          color: "black",
          action: async () => {
            await FlowManager.getInstance().startRoutine(edge.id);
          },
        };

    const menuItems: MenuItem[] = [
      routineMenu,
      {
        label: `Delete`,
        color: "red",
        action: async () => {
          await FlowManager.getInstance().deleteFlow(edge.id);
        },
      },
    ];

    // グローバルストアを更新してコンテキストメニューを表示
    contextMenu.set({ menuItems, position });
  }
  function handleDoubleClick(edge: RoutineEdge) {
    editableEdge = edge; // 編集対象のエッジを設定
    newRoutineName = edge.routine_name; // 現在の名前を新しい名前に設定
  }

  async function handleSave() {
    console.log("should be saved");
    if (editableEdge) {
      editableEdge.routine_name = newRoutineName;
      FlowManager.getInstance().updateEdgeDb(editableEdge);
      let routine_id = editableEdge.associated_routine_id;
      let routine = await DBConnector.getInstance().getRoutine(routine_id);

      if (!routine) return;
      routine.name = newRoutineName;
      await DBConnector.getInstance().updateRoutine(routine);
      await DBConnector.getInstance().updateEdge(editableEdge);

      editableEdge = null;
    }
  }

  async function onCreateNewFlow() {
    await FlowManager.getInstance().createNewFlow();
    //　EdgeからRoutineとedgeをDBに追加したい。
  }

  async function startAllRoutine() {
    await DBConnector.getInstance().startAllRoutine();
  }

  async function stopAllRoutine() {
    await DBConnector.getInstance().stopAllRoutine();
  }
</script>

<div
  style="width: 100%; height:auto; display:flex; flex-direction:row; justify-content:space-around;"
>
  <button
    class="btn btn-primary btn-sm"
    style="display:flex; flex-direction:row; gap:5px; align-items:center; margin-bottom:20px; min-width:auto;"
    on:click={onCreateNewFlow}
  >
    <span class="material-symbols-outlined" style="margin:0px; "> add </span>
    <span style="min-width:100px;">Create Routine</span>
  </button>

  <div
    style="width: 100%; height:auto; display:flex; flex-direction:row; justify-content: end;align-item:center; gap: 20px; height: 100%;"
  >
    <button
      class="btn btn-success btn-sm"
      style="display:flex; flex-direction:row; gap:5px; align-items:center; margin-bottom:20px;"
      on:click={startAllRoutine}
    >
      <span>Start All</span>
    </button>
    <button
      class="btn btn-danger btn-sm"
      style="display:flex; flex-direction:row; gap:5px; align-items:center; margin-bottom:20px;"
      on:click={stopAllRoutine}
    >
      <span>Stop All</span>
    </button>
  </div>
</div>

<table class="table table-hover">
  <thead>
    <tr>
      <th scope="col"></th>
      <th scope="col">Routine Name</th>
      <th scope="col">Status</th>
    </tr>
  </thead>
  <tbody>
    {#each $edgeList as edge, index}
      <tr
        on:contextmenu={(event) => handleContextMenu(event, edge)}
        class={edge.id === $selectedEdge ? "table-active" : ""}
        style="cursor: pointer;"
      >
        <th>{index + 1}</th>
        <td
          on:dblclick={() => handleDoubleClick(edge)}
          on:click={() => FlowManager.getInstance().setSelectEdge(edge.id)}
        >
          {#if editableEdge === edge}
            <input
              type="text"
              bind:value={newRoutineName}
              on:blur={handleSave}
              on:keydown={(e) => e.key === "Enter" && handleSave()}
              style="width: 100%;"
            />
          {:else}
            {edge.routine_name}
          {/if}
        </td>
        <td style="color: {$edgeStatus.get(edge.id) ? 'green' : 'red'}"
          >{$edgeStatus.get(edge.id) ? "Running" : "Stopped"}</td
        >
      </tr>
    {/each}
  </tbody>
</table>

<!-- <RightClickMenu {menuItems} {position} /> -->
