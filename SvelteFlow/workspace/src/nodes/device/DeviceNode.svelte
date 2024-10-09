<script lang="ts">
  import NodeWrapper from "@/components/NodeWrapper.svelte";

  export let label: string;
  export let additionalStyle: string | null;
</script>

<NodeWrapper
  {label}
  style="background-color:#fdcdcd; box-shadow: 0px 0px 5px rgba(0,0,0,0.1); {additionalStyle}"
>
  <slot></slot>
</NodeWrapper>
