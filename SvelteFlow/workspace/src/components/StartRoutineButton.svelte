<script lang="ts">
  import { createEventDispatcher } from "svelte";

  const dispatch = createEventDispatcher();

  function startRoutine() {
    dispatch("start-routine");
  }
</script>

<button class="start-btn btn btn-dark btn-sm" on:click={startRoutine}
  >Start Routine</button
>

<style>
  .start-btn {
    position: fixed;
    top: 20px;
    right: 20px;
    z-index: 1000;
  }
</style>
