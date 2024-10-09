<script lang="ts">
  import { onMount } from "svelte";
  import {
    type IDBDeviceBlock,
    ActionType,
    DeviceType,
  } from "@type/ActionBlockInterface";
  import { Handle, Position } from "@xyflow/svelte";
  import NodeWrapper from "@/components/NodeWrapper.svelte";

  export let data: { action_data: IDBDeviceBlock };
</script>

<NodeWrapper label="Light Actuator" style="background-color: #fdcdcd; ">
  <!-- 親のスロットにタイトルを送信 -->
  <div class="light-actuator">
    <Handle type="target" position={Position.Left} />
  </div>
</NodeWrapper>
