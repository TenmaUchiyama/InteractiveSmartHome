<script lang="ts">
  import { onMount } from "svelte";
  import {
    type IDBDeviceBlock,
    ActionType,
    DeviceType,
  } from "@type/ActionBlockInterface";

  import NodeWrapper from "@/components/NodeWrapper.svelte";
  import { Handle, Position, type NodeProps } from "@xyflow/svelte";

  type $$Props = NodeProps;
  export let data: { action_data: IDBDeviceBlock | null };
</script>

<NodeWrapper label="Toggle Button" style="background-color:#abd991; ">
  <!-- 親のスロットにタイトルを送信 -->
  <div class="toggle-btn-sensor">
    <Handle type="source" position={Position.Right} />
  </div>
</NodeWrapper>

<style>
  body {
    background-color: #abd991;
  }
</style>
