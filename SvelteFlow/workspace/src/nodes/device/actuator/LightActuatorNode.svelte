<script lang="ts">
  import { onMount } from "svelte";
  import {
    type IDBDeviceBlock,
    ActionType,
    DeviceType,
  } from "@type/ActionBlockInterface";
  import { Handle, Position } from "@xyflow/svelte";
  import NodeWrapper from "@/components/NodeWrapper.svelte";
  import { handleStyle } from "@/utils/FlowManager";
  import NodeContent from "@/nodes/NodeContent.svelte";

  export let data: { action_data: IDBDeviceBlock };


  let device_status : string = "";
  let status_color : string = "";


  let onReceiveMqttMessage = (payload:string) => {
  let jsonData = JSON.parse(payload);

  if(jsonData.data_type === "boolean")
{
  device_status = jsonData.value ? "ON" : "OFF"
  status_color = jsonData.value ?  "#00FF00" : "#FF0000"
}
  }
</script>

<NodeWrapper label="Light Actuator" style="background-color: #fdcdcd; ">
  <NodeContent action_data={data.action_data} {onReceiveMqttMessage}>
    <!-- 親のスロットにタイトルを送信 -->
    <div class="light-actuator">
      <h1 style="color: {status_color};">{device_status}</h1>
      <Handle type="target" position={Position.Left} style={handleStyle} />
    </div>
  </NodeContent>
</NodeWrapper>
