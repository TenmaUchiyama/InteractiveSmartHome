<script lang="ts">
  import { type IDBDeviceBlock } from "@type/ActionBlockInterface";

  import NodeWrapper from "@/components/NodeWrapper.svelte";
  import { Handle, Position, type NodeProps } from "@xyflow/svelte";

  type $$Props = NodeProps;
  export let data: { action_data: IDBDeviceBlock | null };
</script>

<NodeWrapper label="Thermometer Sensor" style="background-color:#abd991; ">
  <Handle type="source" position={Position.Right} />
</NodeWrapper>
