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



  let logic_status_input : string = "";
  let status_color_input : string = "";

  
  let logic_status_output : string = "";
  let status_color_output : string = "";



  let onReceiveMqttMessage = (payload:string) => {
  let jsonData = JSON.parse(payload);

  if(jsonData.data_type === "boolean")
{

  console.log(jsonData)
  logic_status_input = !jsonData.value ? "True" : "False"
  status_color_input = !jsonData.value ?  "#00FF00" : "#FF0000"

  logic_status_output = jsonData.value ? "True" : "False"
  status_color_output = jsonData.value ?  "#00FF00" : "#FF0000"
}

}; 


</script>

<LogicNode label="Not Gate Logic" action_data={data.action_data} {onReceiveMqttMessage}>

  <h2 style="color:{status_color_input}">{logic_status_input}</h2>
  <img class="main-img" src="/public/assets/not-gate.png" alt="" />
  <h2 style="color:{status_color_output}">{logic_status_output}</h2>

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
