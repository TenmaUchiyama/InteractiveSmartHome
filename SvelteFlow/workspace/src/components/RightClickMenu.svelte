<script lang="ts">
  import { rightClicked } from "@/store/flowStore";
  import { onMount, onDestroy } from "svelte";

  export let menuItems: Array<{
    label: string;
    color?: string;
    action: (arg: any) => void;
  }> = [];
  export let position = { x: 0, y: 0 };

  let rightClickMenu: HTMLDivElement | null = null;
  export let visible = false;

  function handleItemClick(action: () => void) {
    action();
    visible = false;
  }

  function handleClickOutside(event: MouseEvent) {
    // メニューの外をクリックした場合
    if (rightClickMenu) {
      visible = false;
    }
  }

  onMount(() => {
    window.addEventListener("click", handleClickOutside);
    return () => {
      window.removeEventListener("click", handleClickOutside);
    };
  });
</script>

{#if visible}
  <div
    bind:this={rightClickMenu}
    id="right-click-menu"
    class="context-menu"
    style="position: absolute; top: {position.y}px; left: {position.x}px; background: white; border: 1px solid #ccc; z-index: 1000; "
  >
    <ul>
      {#each menuItems as item}
        <!-- svelte-ignore a11y-click-events-have-key-events -->
        <!-- svelte-ignore a11y-no-noninteractive-element-interactions -->
        <li
          style="color: {item.color}"
          on:click={() => handleItemClick(item.action)}
        >
          {item.label}
        </li>
      {/each}
    </ul>
  </div>
{/if}

<style>
  .context-menu {
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
    border-radius: 4px;
    padding: 10px;
  }
  .context-menu ul {
    list-style-type: none;
    padding: 0;
    margin: 0;
  }
  .context-menu li {
    padding: 8px 12px;
    cursor: pointer;
  }
  .context-menu li:hover {
    background-color: #f0f0f0;
  }
</style>
