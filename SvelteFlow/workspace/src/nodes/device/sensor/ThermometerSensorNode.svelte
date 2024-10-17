<script lang="ts">
  import { type IDBDeviceBlock } from "@type/ActionBlockInterface";
  import { handleStyle } from "@/utils/FlowManager";
  import NodeWrapper from "@/components/NodeWrapper.svelte";
  import { Handle, Position, type NodeProps } from "@xyflow/svelte";
  import NodeContent from "@/nodes/NodeContent.svelte";

  export let data: { action_data: IDBDeviceBlock | null };
</script>

<NodeWrapper label="Thermometer Sensor" style="background-color:#abd991; ">
  <NodeContent action_data={data.action_data}>
    <Handle type="source" position={Position.Right} style={handleStyle} />
  </NodeContent>
</NodeWrapper>
