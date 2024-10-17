<script lang="ts">
  import { selectedEdge, useDnD } from "@/store/flowStore";
  import { NodeType } from "@/type/NodeType";
  import { logics, sensors, actuators } from "@/utils/NodeGenerator";
  import ActionBar from "./ActionBar.svelte";
  import RoutineBar from "./RoutineBar.svelte";
  let currentSelection: "routine" | "action" = "routine";
</script>

<span
  data-bs-toggle="offcanvas"
  data-bs-target="#offcanvasScrolling"
  aria-controls="offcanvasScrolling"
  style="position: relative; z-index: 1000; margin:20px; cursor:pointer;"
  class="material-symbols-outlined menu-btn"
>
  menu
</span>

<div
  class="offcanvas offcanvas-start"
  data-bs-scroll="true"
  data-bs-backdrop="false"
  tabindex="-1"
  id="offcanvasScrolling"
  aria-labelledby="offcanvasScrollingLabel"
>
  <div class="offcanvas-header">
    <!-- svelte-ignore a11y-click-events-have-key-events -->
    <!-- svelte-ignore a11y-no-noninteractive-element-interactions -->
    <div style="gap: 20px; width:100%; display:flex; flex-direction: row;">
      <!-- svelte-ignore a11y-click-events-have-key-events -->
      <h5
        class="offcanvas-title {currentSelection === 'routine'
          ? 'selected-title'
          : ''}"
        on:click={() => (currentSelection = "routine")}
      >
        Routines
      </h5>
      <!-- svelte-ignore a11y-click-events-have-key-events -->
      <!-- svelte-ignore a11y-no-noninteractive-element-interactions -->
      <h5
        class="offcanvas-title {currentSelection === 'action'
          ? 'selected-title'
          : ''}"
        on:click={() => {
          if ($selectedEdge !== null) currentSelection = "action";
        }}
        id="offcanvasScrollingLabel"
      >
        Action Node
      </h5>
    </div>
    <button
      type="button"
      class="btn-close"
      data-bs-dismiss="offcanvas"
      aria-label="Close"
    ></button>
  </div>
  <div class="offcanvas-body">
    <!-- main content -->

    {#if currentSelection === "routine"}
      <RoutineBar />
    {:else if $selectedEdge !== null}
      <ActionBar />
    {/if}
    <!-- <ActionBar /> -->
    <!-- end of main content -->
  </div>
</div>

<style>
  .offcanvas {
    z-index: 1050;
  }
  .offcanvas-title {
    cursor: pointer;
    color: #b8b8b8;
  }
  .offcanvas-title:hover {
    color: #030303;
  }
  .selected-title {
    color: #030303;
    border-bottom: 2px solid #030303;
    padding-bottom: 4px;
  }
  .menu-btn:hover {
    background-color: #d9d7d7;
    border-radius: 5px;
  }
</style>
