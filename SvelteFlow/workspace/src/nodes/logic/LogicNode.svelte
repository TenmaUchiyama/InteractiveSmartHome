<script lang="ts">
  import NodeWrapper from "@/components/NodeWrapper.svelte";
  import type { IActionBlock } from "@/type/ActionBlockInterface";

  import NodeContent from "../NodeContent.svelte";
  export let label: string;

  export let action_data: IActionBlock | null;
</script>

<NodeWrapper {label} style="background-color: #dedede;">
  <NodeContent {action_data}>
    <slot></slot>
  </NodeContent>
</NodeWrapper>
