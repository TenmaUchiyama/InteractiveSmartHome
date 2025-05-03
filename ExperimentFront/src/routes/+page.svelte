<script lang="ts">
	import { onMount } from 'svelte';
	import type { Writable } from 'svelte/store';
	import { writable } from 'svelte/store';

	type LogEntry = {
		task_id: string;
		node: string;
		type: 'agent' | 'tool' | 'postprocess';
		agent_output_type: string;
		input: any;
		output: any;
		timestamp: string;
	};

	let logs: Writable<LogEntry[]> = writable([]);
	let expanded: Writable<Record<string, boolean>> = writable({});
	let showInput: Writable<Record<string, boolean>> = writable({});
	let showOutput: Writable<Record<string, boolean>> = writable({});

	onMount(async () => {
		const res = await fetch('/test_id.json');
		const json = await res.json();
		logs.set(json);

		const initialState = {};
		json.forEach((entry: LogEntry, index: number) => {
			const key = `${index}-${entry.node}`;
			initialState[key] = true;
		});
		expanded.set({ ...initialState });
		showInput.set({ ...initialState });
		showOutput.set({ ...initialState });
	});

	function toggle(key: string, store: Writable<Record<string, boolean>>) {
		store.update((state) => ({ ...state, [key]: !state[key] }));
	}

	function formatContent(content: any): string {
		try {
			if (typeof content === 'string') {
				return JSON.stringify(JSON.parse(content), null, 2);
			} else {
				return JSON.stringify(content, null, 2);
			}
		} catch {
			return typeof content === 'string' ? content : JSON.stringify(content, null, 2);
		}
	}
</script>

<div class="container my-4">
	<h2 class="mb-3">ログファイルビューア</h2>

	{#each $logs as entry, index (entry.timestamp)}
		{#key `${index}-${entry.node}`}
			<div
				class="card mb-3 {entry.type === 'agent'
					? 'card-agent'
					: entry.type === 'tool'
						? 'card-tool'
						: 'card-postprocess'}"
			>
				<div class="card-header d-flex justify-content-between align-items-center">
					<div on:click={() => toggle(`${index}-${entry.node}`, expanded)}>
						<strong>[{entry.type.toUpperCase()}]</strong>
						{entry.node}
					</div>
					<div>
						<button
							class="btn btn-sm btn-outline-primary me-2"
							on:click={() => toggle(`${index}-${entry.node}`, showInput)}
						>
							{#if $showInput[`${index}-${entry.node}`]}Hide Input{:else}Show Input{/if}
						</button>
						<button
							class="btn btn-sm btn-outline-success me-2"
							on:click={() => toggle(`${index}-${entry.node}`, showOutput)}
						>
							{#if $showOutput[`${index}-${entry.node}`]}Hide Output{:else}Show Output{/if}
						</button>
						<button
							class="btn btn-sm btn-outline-secondary"
							on:click={() => toggle(`${index}-${entry.node}`, expanded)}
						>
							{#if $expanded[`${index}-${entry.node}`]}Collapse{:else}Expand{/if}
						</button>
					</div>
				</div>

				{#if $expanded[`${index}-${entry.node}`]}
					<div class="card-body">
						<p><strong>Agent Output Type:</strong> {entry.agent_output_type}</p>

						<p on:click={() => toggle(`${index}-${entry.node}`, showInput)}>
							<strong>Input:</strong>
						</p>
						{#if $showInput[`${index}-${entry.node}`]}
							<pre class="bg-light p-2 rounded"><code>{formatContent(entry.input)}</code></pre>
						{/if}

						<p on:click={() => toggle(`${index}-${entry.node}`, showOutput)}>
							<strong>Output:</strong>
						</p>
						{#if $showOutput[`${index}-${entry.node}`]}
							<pre class="bg-light p-2 rounded"><code>{formatContent(entry.output)}</code></pre>
						{/if}
					</div>
				{/if}
			</div>
		{/key}
	{/each}
</div>

<style>
	.card-agent {
		border-left: 5px solid #0d6efd;
	}
	.card-tool {
		border-left: 5px solid #198754;
	}
	.card-postprocess {
		border-left: 5px solid #fd7e14;
	}
</style>
