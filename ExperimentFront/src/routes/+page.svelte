<script lang="ts">
	import { WebSocketClient } from '$lib/WebSocket';
	import { messages } from '$lib/store';
	import { onMount } from 'svelte';

	const ws = new WebSocketClient('ws://localhost:9876');

	ws.onMessage = (data: string) => {
		console.log(data);
	};
	onMount(() => {
		ws.connect();
	});
</script>

<main>
	<button
		type="button"
		class="btn btn-primary"
		on:click={() => {
			ws.send('Hello');
		}}>Hello</button
	>
</main>

<style>
	main {
		width: 100%;
		height: 100%;
		display: flex;
		justify-content: center;
		align-items: center;
	}
</style>
