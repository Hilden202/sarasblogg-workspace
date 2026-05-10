<script lang="ts">
	import { browser } from '$app/environment';
	import { createEventDispatcher } from 'svelte';
	import { onDestroy, tick } from 'svelte';

	export let open = false;
	export let title = '';
	export let size: 'default' | 'wide' = 'default';

	const dispatch = createEventDispatcher<{ close: void }>();
	let modalElement: HTMLDivElement;

	$: if (browser) {
		document.documentElement.classList.toggle('has-modal-open', open);
		if (open) focusModal();
	}

	function close() {
		dispatch('close');
	}

	async function focusModal() {
		await tick();
		modalElement?.focus();
	}

	function handleKeydown(event: KeyboardEvent) {
		if (open && event.key === 'Escape') {
			close();
		}
	}

	$: if (!open && browser) {
		document.documentElement.classList.remove('has-modal-open');
	}

	onDestroy(() => {
		if (browser) document.documentElement.classList.remove('has-modal-open');
	});
</script>

<svelte:window on:keydown={handleKeydown} />

{#if open}
	<div class="modal-backdrop" role="presentation">
		<button type="button" class="modal-backdrop__button" aria-label="Stäng" on:click={close}></button>
		<div
			bind:this={modalElement}
			class="modal modal--{size}"
			role="dialog"
			aria-modal="true"
			aria-labelledby="modal-title"
			tabindex="-1"
		>
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
		z-index: 90;
		display: grid;
		place-items: center;
		padding: 1rem;
		background: rgba(72, 54, 40, 0.24);
	}

	:global(html.has-modal-open),
	:global(html.has-modal-open body) {
		overflow: hidden;
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
		max-height: min(88vh, 820px);
		overflow: auto;
		padding: 1.25rem;
		border: 1px solid var(--color-border);
		border-radius: var(--radius-card);
		background: var(--color-surface);
		box-shadow: var(--shadow-soft);
	}

	.modal--wide {
		width: min(1040px, 100%);
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

	@media (max-width: 640px) {
		.modal-backdrop {
			align-items: stretch;
			padding: 0.5rem;
		}

		.modal {
			width: 100%;
			max-height: calc(100dvh - 1rem);
			padding: 1rem;
			border-radius: 1rem;
		}

		h2 {
			font-size: 1.55rem;
		}
	}
</style>
