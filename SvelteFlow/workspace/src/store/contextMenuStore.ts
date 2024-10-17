import { writable } from "svelte/store";

export interface MenuItem {
  label: string;
  color?: string;
  action: () => void;
}

export interface ContextMenuState {
  menuItems: MenuItem[];
  position: { x: number; y: number };
}

export const contextMenu = writable<ContextMenuState | null>(null);
