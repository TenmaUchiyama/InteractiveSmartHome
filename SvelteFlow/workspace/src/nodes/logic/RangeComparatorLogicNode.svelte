<script lang="ts">
  import { Handle, Position, type NodeProps } from "@xyflow/svelte";
  import LogicNode from "./LogicNode.svelte"; // 親コンポーネント
  import type { IRangeComparatorLogicBlock } from "@/type/ActionBlockInterface";
  import { handleStyle } from "@/utils/FlowManager";
  import NodeContent from "../NodeContent.svelte";

  export let data: { action_data: IRangeComparatorLogicBlock };

  let operators = [">", "<", ">=", "<="];
  let operatorFrom: ">" | "<" | ">=" | "<=" = ">";
  let operatorTo: ">" | "<" | ">=" | "<=" = "<";

  let valueFrom: number = 0;
  let valueTo: number = 0;
</script>

<LogicNode label="Range Comparator" action_data={data.action_data}>
  <input type="number" bind:value={valueFrom} style="width:10%; margin:0;" />
  <select name="" id="" bind:value={operatorFrom}>
    {#each operators as operator}
      <option value={operator}>{operator}</option>
    {/each}
  </select>
  x
  <select name="" id="" bind:value={operatorTo}>
    {#each operators as operator}
      <option value={operator}>{operator}</option>
    {/each}
  </select>
  <input type="number" bind:value={valueTo} style="width:10%; margin:0;" />

  <Handle type="target" position={Position.Left} style={handleStyle} />
  <Handle type="source" position={Position.Right} style={handleStyle} />
</LogicNode>

<style>
  .name-box {
    display: flex;
    justify-content: center;
    align-items: center;
    flex-direction: column;
    width: 100%;
    font-size: 1.4rem;
  }
  .main {
    display: flex;
    flex-direction: row;
    justify-content: center;
    gap: 20px;
    width: 300px;
  }

  form {
    display: flex;
    flex-direction: row;
    gap: 20px;
    justify-content: center;
    align-items: center;
  }
</style>
