<script lang="ts">
  import Flow from "./flow/Flow.svelte";
  import { SvelteFlowProvider } from "@xyflow/svelte";
  import "@xyflow/svelte/dist/style.css";
  import DnDProvider from "./components/DnDProvider.svelte";
</script>

<div style="height:100vh;">
  <SvelteFlowProvider>
    <DnDProvider>
      <Flow />
    </DnDProvider>
  </SvelteFlowProvider>
</div>
