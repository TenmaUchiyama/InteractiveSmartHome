<script lang="ts">
  import { onMount } from "svelte";
  import {
    type IDBDeviceBlock,
    ActionType,
    DeviceType,
  } from "@type/ActionBlockInterface";
  import { Handle, Position } from "@xyflow/svelte";
  import NodeWrapper from "@/components/NodeWrapper.svelte";
  import { handleStyle } from "@/utils/FlowManager";
  import NodeContent from "@/nodes/NodeContent.svelte";

  export let data: { action_data: IDBDeviceBlock };
</script>

<NodeWrapper label="Light Actuator" style="background-color: #fdcdcd; ">
  <NodeContent action_data={data.action_data}>
    <!-- 親のスロットにタイトルを送信 -->
    <div class="light-actuator">
      <Handle type="target" position={Position.Left} style={handleStyle} />
    </div>
  </NodeContent>
</NodeWrapper>
