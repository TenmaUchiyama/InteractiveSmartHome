<script lang="ts">
  import { useDnD } from "@/store/flowStore";
  import { NodeType } from "@/type/NodeType";
  import { logics, sensors, actuators } from "@/utils/NodeGenerator";

  const type = useDnD();

  const onDragStart = (event: DragEvent, nodeType: string) => {
    if (!event.dataTransfer) {
      return null;
    }

    event.dataTransfer.effectAllowed = "move";

    type.set(nodeType);
  };
</script>

<aside>
  <h3>Logic</h3>
  <div class="grid-container">
    {#each logics as logic}
      <!-- svelte-ignore a11y-no-static-element-interactions -->
      <div
        class="box"
        style="background-color: #e0e0e0;"
        on:dragstart={(event) => onDragStart(event, logic.type)}
        draggable={true}
      >
        {logic.label}
      </div>
    {/each}
  </div>

  <h3>Sensor</h3>
  <div class="grid-container">
    {#each sensors as sensor}
      <!-- svelte-ignore a11y-no-static-element-interactions -->
      <div
        class="box"
        style="background-color: #abd991;"
        on:dragstart={(event) => onDragStart(event, sensor.type)}
        draggable={true}
      >
        {sensor.label}
      </div>
    {/each}
  </div>

  <h3>Actuator</h3>
  <div class="grid-container">
    {#each actuators as actuator}
      <!-- svelte-ignore a11y-no-static-element-interactions -->
      <div
        class="box"
        style="background-color: #fdcdcd;"
        on:dragstart={(event) => onDragStart(event, actuator.type)}
        draggable={true}
      >
        {actuator.label}
      </div>
    {/each}
  </div>
</aside>

<style>
  aside {
    width: 100%;
    font-size: 12px;
    display: flex;
    flex-direction: column;
    padding: 1rem;
  }

  .grid-container {
    display: grid;
    grid-template-columns: repeat(
      auto-fill,
      minmax(150px, 1fr)
    ); /* カラムの最小幅を設定 */
    gap: 10px; /* 各ボックスの間に隙間を追加 */
    margin-bottom: 30px;
  }

  .box {
    background-color: #e0e0e0;
    border: 1px solid #111;
    padding: 1rem;
    text-align: center;
    border-radius: 5px;
  }
</style>
