<script lang="ts">
  import { Handle, Position, type NodeProps } from "@xyflow/svelte";
  import LogicNode from "./LogicNode.svelte"; // 親コンポーネント
  import type { ISimpleComparatorLogicBlock } from "@/type/ActionBlockInterface";
  import { handleStyle } from "@/utils/FlowManager";
  import NodeContent from "../NodeContent.svelte";

  export let data: { action_data: ISimpleComparatorLogicBlock };

  let operators = [">", "<", "=", "!=", ">=", "<="];
  let selectedOperator: string = operators[0];
  let selectedValue: number = 0;

  $: {
    data.action_data.value = selectedValue;

    if (data.action_data) data.action_data.operator = operators[0] as any;
  }
</script>

<LogicNode label="Simple Comparator" action_data={data.action_data}>
  <select name="" id="" bind:value={selectedOperator}>
    {#each operators as operator}
      <option value={operator}>{operator}</option>
    {/each}
  </select>
  <input type="number" bind:value={selectedValue} />

  <Handle type="target" position={Position.Left} style={handleStyle} />
  <Handle type="source" position={Position.Right} style={handleStyle} />
</LogicNode>
