<script lang="ts">
  import { ActionType, type IActionBlock } from "@/type/ActionBlockInterface";
  import MqttBridge from "@/utils/MqttConnector";
  import { onMount } from "svelte";

  export let action_data: IActionBlock | null;
  export let onReceiveMqttMessage : (payload : string) => void;

  let editableName: string = ""; // 編集対象の名前


  onMount(() => {


     MqttBridge.getInstance().subscribeToTopic("mrflow/" + action_data!.id, onReceiveMqttMessage )
  })

  let isEditing: boolean = false; // 編集モードのフラグ

  // ダブルクリックで編集モードに入る
  function handleDoubleClick() {
    editableName = action_data ? action_data.name : ""; // 現在の名前を設定
    isEditing = true; // 編集モードをオン
  }

  // 保存処理
  function handleSave() {
    if (editableName.trim() !== "") {
      if (action_data) action_data.name = editableName; // 名前を更新
      // ここで必要に応じてデータベースへの更新処理を追加
    }
    isEditing = false; // 編集モードをオフ
  }
</script>

<main>
  <div class="name-box">
    <!-- svelte-ignore a11y-no-static-element-interactions -->
    <span class="name" on:dblclick={handleDoubleClick}>
      {#if isEditing}
        <input
          type="text"
          bind:value={editableName}
          on:blur={handleSave}
          on:keydown={(e) => e.key === "Enter" && handleSave()}
          style="width: 100%;"
        />
      {:else}
        {action_data?.name}
      {/if}
    </span>
  </div>
  <br />
  <form style="width:auto;">
    <slot />
  </form>
</main>

<style>
  .name-box {
    display: flex;
    justify-content: center;
    align-items: center;
    flex-direction: column;
  }
  .name {
    font-size: 1rem;
  }
  .main {
    display: flex;
    flex-direction: row;
    justify-content: center;
  }

  form {
    display: flex;
    flex-direction: row;
    gap: 20px;
    justify-content: center;
    align-items: center;
  }
</style>
