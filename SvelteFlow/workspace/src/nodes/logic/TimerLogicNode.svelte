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
    type ITimerLogicBlock,
  } from "@type/ActionBlockInterface";
  import NodeContent from "../NodeContent.svelte";
  import { handleStyle } from "@/utils/FlowManager";
  export let id: string;
  export let data: { action_data: ITimerLogicBlock };

  const connections = useHandleConnections({ nodeId: id, type: "target" });
  $: isConnectable = $connections.length === 0;
</script>

<LogicNode label="Timer Logic" action_data={data.action_data}>
  <div class="timepicker">
    <div>
      time (s): <strong>{data.action_data?.waitTime}</strong>
    </div>
    <input
      class="nodrag"
      type="number"
      min="0"
      on:input={(evt) => {
        if (data.action_data)
          data.action_data.waitTime = parseFloat(evt.target?.value);
      }}
      value={data.action_data?.waitTime}
    />
  </div>
  <Handle
    type="target"
    position={Position.Left}
    style={handleStyle}
    {isConnectable}
  />
  <Handle type="source" position={Position.Right} style={handleStyle} />
</LogicNode>

<style>
  .timepicker {
    padding: 0.5rem;
    background: #fff;
    border-radius: 0.125rem;
    font-size: 0.7rem;
  }

  h3 {
    font-size: 0.9rem;
    font-weight: normal;
    margin: 0.5rem 0;
  }
</style>
