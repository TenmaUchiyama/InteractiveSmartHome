<script lang="ts">
  import NodeContent from "../NodeContent.svelte";
  import NodeWrapper from "@/components/NodeWrapper.svelte";
  import { type IActionBlock } from "@/type/ActionBlockInterface";

  export let action_data: IActionBlock | null;
  export let label: string;
</script>

<NodeWrapper label="Schedule Api" style="background-color:#aec9f5; ">
  <NodeContent {action_data}>
    <slot></slot>
  </NodeContent>
</NodeWrapper>
