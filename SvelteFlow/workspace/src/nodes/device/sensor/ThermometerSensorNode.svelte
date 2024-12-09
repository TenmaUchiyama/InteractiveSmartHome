<script lang="ts">
  import { type IDBDeviceBlock } from "@type/ActionBlockInterface";
  import { handleStyle } from "@/utils/FlowManager";
  import NodeWrapper from "@/components/NodeWrapper.svelte";
  import { Handle, Position, type NodeProps } from "@xyflow/svelte";
  import NodeContent from "@/nodes/NodeContent.svelte";

  export let data: { action_data: IDBDeviceBlock | null };


  let temperatureText : string = "0";
 const onReceiveMqttMessage = (payload:string) => {
    console.log("RECEIVED")
    let jsonData = JSON.parse(payload);

    if(jsonData.data_type === "number")
    {
      temperatureText = jsonData.value.toString();
    }
    

  }; 



</script>

<NodeWrapper label="Thermometer Sensor" style="background-color:#abd991; ">
  <NodeContent action_data={data.action_data} {onReceiveMqttMessage}>
    <h1>{temperatureText}</h1>
    <Handle type="source" position={Position.Right} style={handleStyle} />
  </NodeContent>
</NodeWrapper>
