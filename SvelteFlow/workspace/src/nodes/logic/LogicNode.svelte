<script lang="ts">
  import NodeWrapper from "@/components/NodeWrapper.svelte";
  import { NodeToolbar } from "@xyflow/svelte";

  export let label: string;
</script>

<NodeWrapper {label} style="background-color: #dedede;">
  <slot></slot>
</NodeWrapper>

<style>
  .logic-node {
    background-color: #dedede;
  }
</style>
