<script lang="ts">
  import { onMount } from "svelte";
  import {
    type IDBDeviceBlock,
    ActionType,
    DeviceType,
  } from "@type/ActionBlockInterface";

  import NodeWrapper from "@/components/NodeWrapper.svelte";
  import { Handle, Position, type NodeProps } from "@xyflow/svelte";
  import { handleStyle } from "@/utils/FlowManager";
  import NodeContent from "@/nodes/NodeContent.svelte";

  export let data: { action_data: IDBDeviceBlock };
</script>

<NodeWrapper label="Toggle Button" style="background-color:#abd991; ">
  <NodeContent action_data={data.action_data}>
    <div class="toggle-btn-sensor">
      <Handle type="source" position={Position.Right} style={handleStyle} />
    </div>
  </NodeContent>
</NodeWrapper>

<style>
  body {
    background-color: #abd991;
  }
</style>
