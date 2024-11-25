<script lang="ts">
  import { Handle, Position, type NodeProps } from "@xyflow/svelte";
  import LogicNode from "./LogicNode.svelte"; // 親コンポーネント
  import type { ISimpleComparatorLogicBlock } from "@/type/ActionBlockInterface";
  import { handleStyle } from "@/utils/FlowManager";
  import NodeContent from "../NodeContent.svelte";

  export let data: { action_data: ISimpleComparatorLogicBlock };

  let comparators = [">", "<", "=", "!=", ">=", "<="];
  let selectedComparator: string = comparators[0];
  let selectedValue: number = 0;



  $:{
    console.log(data.action_data)
  }

  let logic_status : string = "";
  let status_color : string = "";
  let onReceiveMqttMessage = (payload:string) => {
  let jsonData = JSON.parse(payload);

  if(jsonData.data_type === "boolean")
{
  logic_status = jsonData.value ? "ON" : "OFF"
  status_color = jsonData.value ?  "#00FF00" : "#FF0000"
}

}; 


</script>

<LogicNode label="Simple Comparator" action_data={data.action_data} {onReceiveMqttMessage}>


  <div style = "display:flex; flex-direction: column;">

    <h1 style="color: {status_color}; text-align:center;">{logic_status}</h1>



    <div style = "display:flex; flex-direction: row; gap: 10px; ">
      <select name="" id="" bind:value={data.action_data.comparator}>
        {#each comparators as comparator}
          <option value={comparator}>{comparator}</option>
        {/each}
      </select>
      <input type="number" bind:value={data.action_data.value} />
      </div>

  </div>

 

  <Handle type="target" position={Position.Left} style={handleStyle} />
  <Handle type="source" position={Position.Right} style={handleStyle} />
</LogicNode>
