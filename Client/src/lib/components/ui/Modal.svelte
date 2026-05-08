<script lang="ts">
	import { createEventDispatcher } from 'svelte';

	export let open = false;
	export let title = '';

	const dispatch = createEventDispatcher<{ close: void }>();

	function close() {
		dispatch('close');
	}

	function handleKeydown(event: KeyboardEvent) {
		if (open && event.key === 'Escape') {
			close();
		}
	}
</script>

<svelte:window on:keydown={handleKeydown} />

{#if open}
	<div class="modal-backdrop" role="presentation">
		<button type="button" class="modal-backdrop__button" aria-label="Stäng" on:click={close}></button>
		<div class="modal" role="dialog" aria-modal="true" aria-labelledby="modal-title" tabindex="-1">
			<header>
				<h2 id="modal-title">{title}</h2>
				<button type="button" aria-label="Stäng" on:click={close}>x</button>
			</header>
			<slot />
		</div>
	</div>
{/if}

<style>
	.modal-backdrop {
		position: fixed;
		inset: 0;
		z-index: 70;
		display: grid;
		place-items: center;
		padding: 1rem;
		background: rgba(72, 54, 40, 0.24);
	}

	.modal-backdrop__button {
		position: absolute;
		inset: 0;
		width: 100%;
		height: 100%;
		border: 0;
		background: transparent;
	}

	.modal {
		position: relative;
		z-index: 1;
		width: min(680px, 100%);
		max-height: min(82vh, 760px);
		overflow: auto;
		padding: 1.25rem;
		border: 1px solid var(--color-border);
		border-radius: var(--radius-card);
		background: var(--color-surface);
		box-shadow: var(--shadow-soft);
	}

	header {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 1rem;
		margin-bottom: 1rem;
	}

	h2 {
		margin: 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: 2rem;
	}

	button {
		width: 2.25rem;
		height: 2.25rem;
		border: 1px solid var(--color-border);
		border-radius: 999px;
		background: var(--color-surface);
		color: var(--color-muted);
		font-weight: 900;
	}
</style>
