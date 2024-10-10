<script lang="ts">
  import { Handle, Position } from "@xyflow/svelte";
  import LogicNode from "./LogicNode.svelte"; // 親コンポーネント
  import type { ISimpleComparatorLogicBlock } from "@/type/ActionBlockInterface";

  export let data: { action_data: ISimpleComparatorLogicBlock };

  let operators = [">", "<", "=", "!=", ">=", "<="];
  let selectedOperator: string = operators[0];
  let selectedValue: number = 0;

  $: {
    data.action_data.value = selectedValue;
    data.action_data.operator = operators[0];
  }
</script>

<LogicNode label="Simple Comparator">
  <main style="display: flex; flex-direction: row; gap:20px;">
    <select name="" id="" bind:value={selectedOperator}>
      {#each operators as operator}
        <option value={operator}>{operator}</option>
      {/each}
    </select>
    <input type="number" bind:value={selectedValue} />
  </main>
  <Handle type="target" position={Position.Left} />
  <Handle type="source" position={Position.Right} />
</LogicNode>
