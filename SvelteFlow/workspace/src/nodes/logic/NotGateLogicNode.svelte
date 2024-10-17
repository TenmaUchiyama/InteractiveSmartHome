<script lang="ts">
  import {
    Handle,
    Position,
    NodeToolbar,
    useHandleConnections,
  } from "@xyflow/svelte";
  import LogicNode from "./LogicNode.svelte"; // 親コンポーネント
  import {
    ActionType,
    type INotGateLogicBlock,
    type ITimerLogicBlock,
  } from "@type/ActionBlockInterface";
  import NodeContent from "../NodeContent.svelte";
  import { handleStyle } from "@/utils/FlowManager";
  // export let data: { action_data: INotGateLogicBlock };

  export let id: string;
  export let data: { action_data: INotGateLogicBlock };

  const connections = useHandleConnections({ nodeId: id, type: "target" });

  $: isConnectable = $connections.length === 0;
</script>

<LogicNode label="Not Gate Logic" action_data={data.action_data}>
  <img class="main-img" src="/public/not-gate.png" alt="" />

  <Handle
    type="target"
    position={Position.Left}
    style={handleStyle}
    {isConnectable}
  />
  <Handle type="source" position={Position.Right} style={handleStyle} />
</LogicNode>

<style>
  .main-img {
    width: 100px;
    height: 100px;
  }
</style>
