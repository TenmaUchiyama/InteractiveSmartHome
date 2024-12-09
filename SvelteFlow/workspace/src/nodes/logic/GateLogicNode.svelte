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

  
  let logic_status : string = "";
  let status_color : string = "black";
  let onReceiveMqttMessage = (payload:string) => {
  let jsonData = JSON.parse(payload);

  if(jsonData.data_type === "boolean")
{
  logic_status = jsonData.value ? "ON" : "OFF"
  status_color = jsonData.value ?  "#00FF00" : "#FF0000"
}

}; 

  
</script>

<LogicNode label="Not Gate Logic" action_data={data.action_data} {onReceiveMqttMessage}>
    <div style = "display:flex; flex-direction: column;">

    <h1 style="color: {status_color}; text-align:center;">{logic_status}</h1>



    <div style = "display:flex; flex-direction: row; align-items:center; gap: 10px; ">
  <select class="form-select" style="max-height:40px; "bind:value={data.action_data.logic_operator}>
    {#each operators as operator}
      <option value={operator}>{operator}</option>
    {/each}
  </select>
  <img
    class="main-img"
    src="/public/assets/{data.action_data.logic_operator === 'AND' ? 'and' : 'or'}-gate.png"
    alt=""
  />
        </div>

  </div>

  <Handle type="target" position={Position.Left} style={handleStyle} />
  <Handle type="source" position={Position.Right} style={handleStyle} />
</LogicNode>

<style>
  .main-img {
    width: 100px;
    height: 100px;
  }
</style>
