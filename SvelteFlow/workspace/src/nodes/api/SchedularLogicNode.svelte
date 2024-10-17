<script lang="ts">
  import type { IScheduleLogicBlock } from "@/type/ActionBlockInterface";
  import ApiNode from "./ApiNode.svelte";
  import { handleStyle } from "@/utils/FlowManager";
  import { Handle, Position } from "@xyflow/svelte";

  export let data: { action_data: IScheduleLogicBlock };

  let interval = 1;
  let unit = "persec";

  $: {
    let cronData: string = "";
    switch (unit) {
      case "persec":
        cronData = `*/${interval} * * * * *`;
        break;
      case "permin":
        cronData = `*/${interval} * * * *`;
        break;
      case "perhour":
        cronData = `0 */${interval} * * *`;
        break;
      default:
        cronData = "* * * * *";
        break;
    }

    data.action_data.cronExpression = cronData;
  }
</script>

<ApiNode label="Schedule Api" action_data={data.action_data}>
  <label for="input-interval">Select Interval</label>
  <div
    id="input-interval"
    style="display:flex; flex-direction:row; justify-content:center; align-items: center; gap:10px;"
  >
    <div>
      <input
        type="number"
        class="form-control"
        placeholder="Intervals"
        autocomplete="off"
        bind:value={interval}
      />
    </div>

    <select bind:value={unit} class="form-select">
      <option value="persec">per sec</option>
      <option value="permin">per min</option>
      <option value="perhour">per hour</option>
    </select>
  </div>

  <Handle type="source" position={Position.Right} style={handleStyle} />
</ApiNode>

<style>
  label {
    margin-right: 10px;
  }
</style>
