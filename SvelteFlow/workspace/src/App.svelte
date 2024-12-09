<script lang="ts">
  import Flow from "./flow/Flow.svelte";
  import { SvelteFlowProvider } from "@xyflow/svelte";
  import "@xyflow/svelte/dist/style.css";
  import DnDProvider from "./components/DnDProvider.svelte";
  import { onMount } from "svelte";
  import FlowManager from "./utils/FlowManager";
  import SocketConnector from "./utils/SocketConnector";
  import RightClickMenu from "./components/RightClickMenu.svelte";
  import { edges, selectedEdge } from "./store/flowStore";
  import MqttBridge from "./utils/MqttConnector";

  onMount(async () => {
    await MqttBridge.getInstance().setupMqtt();
    await FlowManager.getInstance().initStore();
    FlowManager.getInstance().initScene();
    SocketConnector.getInstance().init();
  });
</script>

<div style="height:100vh;">
  <SvelteFlowProvider>
    <RightClickMenu />
    <DnDProvider>
      <Flow />
    </DnDProvider>
  </SvelteFlowProvider>
</div>
