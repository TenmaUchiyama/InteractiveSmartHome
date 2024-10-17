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
    type IGateLogicBlock,
    type INotGateLogicBlock,
    type ITimerLogicBlock,
  } from "@type/ActionBlockInterface";
  import NodeContent from "../NodeContent.svelte";
  import { handleStyle } from "@/utils/FlowManager";
  // export let data: { action_data: INotGateLogicBlock };

  export let data: { action_data: IGateLogicBlock };

  let operators: string[] = ["AND", "OR"];
  let currentOperator: string = data.action_data.logic_operator;
</script>

<LogicNode label="Not Gate Logic" action_data={data.action_data}>
  <select class="form-select" bind:value={currentOperator}>
    {#each operators as operator}
      <option value={operator}>{operator}</option>
    {/each}
  </select>
  <img
    class="main-img"
    src="/public/{currentOperator === 'AND' ? 'and' : 'or'}-gate.png"
    alt=""
  />
  <Handle type="target" position={Position.Left} style={handleStyle} />
  <Handle type="source" position={Position.Right} style={handleStyle} />
</LogicNode>

<style>
  .main-img {
    width: 100px;
    height: 100px;
  }
</style>
