<script lang="ts">
  import { Handle, Position, type NodeProps } from "@xyflow/svelte";
  import LogicNode from "./LogicNode.svelte"; // 親コンポーネント
  import type { IRangeComparatorLogicBlock } from "@/type/ActionBlockInterface";
  import { handleStyle } from "@/utils/FlowManager";
  import NodeContent from "../NodeContent.svelte";

  export let data: { action_data: IRangeComparatorLogicBlock };

  let comparators = [">", "<", ">=", "<="];
  let comparatorFrom: ">" | "<" | ">=" | "<=" = ">";
  let comparatorTo: ">" | "<" | ">=" | "<=" = "<";

  let valueFrom: number = 0;
  let valueTo: number = 0;

  let logic_status : string =""
  let status_color: string = ""

  let onReceiveMqttMessage = (payload:string) => {
  let jsonData = JSON.parse(payload);

  if(jsonData.data_type === "boolean")
{
  logic_status = jsonData.value ? "ON" : "OFF"
  status_color = jsonData.value ?  "#00FF00" : "#FF0000"
}

}; 




</script>

<LogicNode label="Range Comparator" action_data={data.action_data} {onReceiveMqttMessage}>


  <div style = "display:flex; flex-direction: column;">

    <h1 style="color: {status_color}; text-align:center;">{logic_status}</h1>



    <div style = "display:flex; flex-direction: row; gap: 10px; justify-content:center;">
  
    
        <input type="number" bind:value={data.action_data.from} style="width:100px; margin:0;" />
        <select name="" id="" bind:value={data.action_data.comparatorFrom}>
          {#each comparators as operator}
            <option value={operator}>{operator}</option>
          {/each}
        </select>
        x
        <select name="" id="" bind:value={data.action_data.comparatorTo}>
          {#each comparators as operator}
            <option value={operator}>{operator}</option>
          {/each}
        </select>
        <input type="number" bind:value={data.action_data.to} style="width:100px; margin:0;" />

      </div>

  </div>

  
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
    width: 150px;
  }

  form {
    display: flex;
    flex-direction: row;
    gap: 20px;
    justify-content: center;
    align-items: center;
  }
</style>
