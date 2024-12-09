<script lang="ts">
  import { onMount } from "svelte";
  import {
    type IDBDeviceBlock,
    ActionType,
    DeviceType,
  } from "@type/ActionBlockInterface";

  import NodeWrapper from "@/components/NodeWrapper.svelte";
  import { Handle, Position, type NodeProps } from "@xyflow/svelte";
  import { handleStyle } from "@/utils/FlowManager";
  import NodeContent from "@/nodes/NodeContent.svelte";

  export let data: { action_data: IDBDeviceBlock };

  let device_status: string = ""
  let status_color: string = "" 
  
let onReceiveMqttMessage = (payload:string) => {
  let jsonData = JSON.parse(payload);

  if(jsonData.data_type === "boolean")
{
  device_status = jsonData.value ? "ON" : "OFF"
  status_color = jsonData.value ?  "#00FF00" : "#FF0000"
}

}; 


function toBoolean(value: true | "True" | 1 | false | "False" | 0): boolean {
  return value === true || value === "True" || value === 1;
}
</script>

<NodeWrapper label="Toggle Button" style="background-color:#abd991; ">
  <NodeContent action_data={data.action_data} {onReceiveMqttMessage}>
    <div class="toggle-btn-sensor">
      <h1 style="color: {status_color};">{device_status}</h1>
      <Handle type="source" position={Position.Right} style={handleStyle} />
    </div>
  </NodeContent>
</NodeWrapper>

<style>
  body {
    background-color: #abd991;
  }
</style>
