<script lang="ts">
  import { Handle, Position } from "@xyflow/svelte";
  import LogicNode from "./LogicNode.svelte"; // 親コンポーネント
  import {
    ActionType,
    type ITimerLogicBlock,
  } from "@type/ActionBlockInterface";
  import { onMount } from "svelte";

  export let data: { action_data: ITimerLogicBlock };
</script>

<LogicNode label="Timer">
  <!-- 緑色背景のLogicNodeを使用 -->
  <div class="timepicker">
    <div>
      time (s): <strong>{data.action_data?.waitTime}</strong>
    </div>
    <input
      class="nodrag"
      type="number"
      min="0"
      on:input={(evt) => {
        if (data.action_data)
          data.action_data.waitTime = parseFloat(evt.target?.value);
      }}
      value={data.action_data?.waitTime}
    />
    <Handle type="target" position={Position.Left} />

    <Handle type="source" position={Position.Right} />
  </div>
</LogicNode>

<style>
  .timepicker {
    padding: 0.5rem;
    background: #fff;
    border-radius: 0.125rem;
    font-size: 0.7rem;
  }

  h3 {
    font-size: 0.9rem; /* タイトルを小さくする */
    font-weight: normal; /* 普通の太さにする */
    margin: 0.5rem 0; /* 上下の余白を調整 */
  }
</style>
