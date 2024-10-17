<script lang="ts">
  import NodeWrapper from "@/components/NodeWrapper.svelte";
  import type { IDeviceData } from "@/type/ActionBlockInterface";
  import { DBConnector } from "@/utils/DBConnector";
  import { onMount } from "svelte";
  import NameField from "../NodeContent.svelte";
  import NodeContent from "../NodeContent.svelte";

  export let label: string;
  export let additionalStyle: string | null;
  export let deviceId: string;

  let deviceData: IDeviceData | null = null;

  onMount(async () => {
    deviceData = await DBConnector.getInstance().getDevice(deviceId);
  });
</script>

<NodeWrapper
  {label}
  style="background-color:#fdcdcd; box-shadow: 0px 0px 5px rgba(0,0,0,0.1); {additionalStyle}"
>
  <!-- <NodeContent action_data={deviceData}>
    <slot></slot>
  </NodeContent> -->
</NodeWrapper>
