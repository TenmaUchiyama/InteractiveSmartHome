<script lang="ts">
  import NodeWrapper from "@/components/NodeWrapper.svelte";
  import type { IActionBlock } from "@/type/ActionBlockInterface";

  import NodeContent from "../NodeContent.svelte";
  export let label: string;

  export let action_data: IActionBlock | null;


  export let onReceiveMqttMessage : (payload:string) => void; 
</script>

<NodeWrapper {label} style="background-color: #dedede;">
  <NodeContent {action_data} {onReceiveMqttMessage}>
    <slot></slot>
  </NodeContent>
</NodeWrapper>
