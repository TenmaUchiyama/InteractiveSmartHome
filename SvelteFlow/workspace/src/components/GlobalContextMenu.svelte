<!-- src/components/GlobalContextMenu.svelte -->
<script lang="ts">
  import { onMount, onDestroy } from "svelte";
  import { contextMenu, type MenuItem } from "@/store/contextMenuStore";
  import { get } from "svelte/store";

  let menuItems: MenuItem[] = [];
  let position = { x: 0, y: 0 };
  let visible = false;
  let contextMenuElement: HTMLDivElement | null = null;

  const unsubscribe = contextMenu.subscribe((state) => {
    if (state) {
      menuItems = state.menuItems;
      position = state.position;
      visible = true;
    } else {
      visible = false;
    }
  });

  function handleItemClick(action: () => void) {
    action();
    contextMenu.set(null); // メニューを閉じる
  }

  function handleClickOutside(event: MouseEvent) {
    if (
      contextMenuElement &&
      !contextMenuElement.contains(event.target as Node)
    ) {
      contextMenu.set(null); // メニューを閉じる
    }
  }

  onMount(() => {
    window.addEventListener("click", handleClickOutside);
  });

  onDestroy(() => {
    window.removeEventListener("click", handleClickOutside);
    unsubscribe();
  });
</script>

{#if visible}
  <div
    bind:this={contextMenuElement}
    class="context-menu"
    style="position: absolute; top: {position.y}px; left: {position.x}px; background: white; border: 1px solid #ccc; z-index: 1000;"
  >
    <ul>
      <!-- svelte-ignore a11y-click-events-have-key-events -->
      <!-- svelte-ignore a11y-no-noninteractive-element-interactions -->
      {#each menuItems as item}
        <!-- svelte-ignore a11y-click-events-have-key-events -->
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
    transition:
      opacity 0.2s ease-in-out,
      transform 0.2s ease-in-out;
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
