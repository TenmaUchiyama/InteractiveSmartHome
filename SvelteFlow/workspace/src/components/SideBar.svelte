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
    console.log("DRAG STARTED: ", nodeType);

    type.set(nodeType);
  };
</script>

<span
  data-bs-toggle="offcanvas"
  data-bs-target="#offcanvasScrolling"
  aria-controls="offcanvasScrolling"
  style="position: relative; z-index: 1000; margin:20px; cursor:pointer;"
  class="material-symbols-outlined menu-btn"
>
  menu
</span>

<div
  class="offcanvas offcanvas-start"
  data-bs-scroll="true"
  data-bs-backdrop="false"
  tabindex="-1"
  id="offcanvasScrolling"
  aria-labelledby="offcanvasScrollingLabel"
>
  <div class="offcanvas-header">
    <h5 class="offcanvas-title" id="offcanvasScrollingLabel">Add Node</h5>
    <button
      type="button"
      class="btn-close"
      data-bs-dismiss="offcanvas"
      aria-label="Close"
    ></button>
  </div>
  <div class="offcanvas-body">
    <!-- main contents here -->

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

    <!-- end of main contents -->
  </div>
</div>

<style>
  .offcanvas {
    z-index: 1050; /* Bootstrapのオフキャンバスのデフォルトのz-indexを上回る */
  }

  .menu-btn:hover {
    background-color: #d9d7d7; /* ホバー時の背景色 */
    border-radius: 5px; /* 角丸 */
  }

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
