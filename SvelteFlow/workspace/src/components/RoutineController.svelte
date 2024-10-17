<script lang="ts">
  import {
    edgeList,
    edgeStatus,
    selectedEdge,
    selectedEdgeIndex,
  } from "@/store/flowStore";
  import type { RoutineEdge } from "@/type/NodeType";
  import FlowManager from "@/utils/FlowManager";
  import { createEventDispatcher } from "svelte";
  import { get } from "svelte/store";

  const dispatch = createEventDispatcher();

  function startRoutine() {
    dispatch("start-routine");
  }

  function stopRoutine() {
    dispatch("stop-routine");
  }

  let currentStatus: boolean = false;
  let currentRoutine: string = "";

  $: {
    let edgeId: string | null = $selectedEdge;
    if (edgeId) {
      let edgeList = $edgeList;
      let getCurrentEdge = edgeList.find(
        (edge: RoutineEdge) => edge.id === edgeId
      );

      if (getCurrentEdge) {
        currentRoutine = getCurrentEdge.routine_name;
        let getRoutineStatus = $edgeStatus.get(edgeId);
        if (getRoutineStatus !== undefined) currentStatus = getRoutineStatus;
      }
    }
  }

  function handleRoutineChange(left: boolean) {
    selectedEdgeIndex.update((ind: number) => {
      const getEdges = get(edgeList);
      let edgeListLength = getEdges.length;
      let newInd = (ind + (left ? -1 : 1) + edgeListLength) % edgeListLength;

      let getEdgeId = getEdges[newInd].id;

      FlowManager.getInstance().setSelectEdge(getEdgeId);
      return newInd;
    });
  }
</script>

<div class="routine_controller">
  <div class="controller">
    <!-- svelte-ignore a11y-click-events-have-key-events -->
    <!-- svelte-ignore a11y-no-static-element-interactions -->
    <span
      class="arrow-btn material-symbols-outlined"
      on:click={() => {
        handleRoutineChange(true);
      }}>arrow_back_ios</span
    >
    <p style="text-align:center; font-size:0.9rem;">{currentRoutine}</p>
    <!-- svelte-ignore a11y-click-events-have-key-events -->
    <!-- svelte-ignore a11y-no-static-element-interactions -->
    <span
      class="arrow-btn material-symbols-outlined"
      on:click={() => {
        handleRoutineChange(false);
      }}
    >
      arrow_forward_ios</span
    >
  </div>

  {#if currentStatus}
    <button class="start-btn btn btn-sm btn-danger" on:click={stopRoutine}
      >Stop Routine</button
    >
  {:else}
    <button class="start-btn btn btn-sm btn-primary" on:click={startRoutine}
      >Start Routine</button
    >
  {/if}

  <span
    class="status-label"
    style="font-size: 0.8rem; color: {currentStatus ? 'green' : 'red'}"
    >{currentStatus ? "Running" : "Stopped"}</span
  >
</div>

<style>
  /* .start-btn {
    background-color: #aac3f4;
  } */

  .start-btn:hover {
    background-color: #d8d5d5;
  }

  .start-btn:active {
    background-color: #b3b3b3;
  }
  .routine_controller {
    position: fixed;
    top: 20px;
    right: 20px;
    z-index: 1000;
    display: flex;
    justify-content: flex-end;
    align-items: center;
    flex-direction: column;
    gap: 10px;
  }

  .controller {
    display: flex;
    justify-content: flex-end;
    align-items: center;
    flex-direction: row;
  }
  .controller p {
    display: flex;
    align-items: center;
    margin: 0;
  }
  .arrow-btn {
    font-size: 1rem;
    cursor: pointer;
    /* background-color: #d8d5d5; */
    border-radius: 5px;
    padding: 2px;
    margin: 2px;
  }

  .arrow-btn:hover {
    background-color: #d8d5d5;
  }

  .arrow-btn:active {
    background-color: #b3b3b3;
  }
</style>
