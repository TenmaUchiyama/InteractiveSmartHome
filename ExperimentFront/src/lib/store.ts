import { writable, type Writable } from 'svelte/store';

export let messages: Writable<string[]> = writable([]);
