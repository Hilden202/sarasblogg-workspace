<script lang="ts">
	import { toasts } from '$lib/stores/toastStore';
</script>

<div class="toast-region" aria-live="polite" aria-atomic="true">
	{#each $toasts as toast (toast.id)}
		<div class="toast toast--{toast.kind}">
			<p>{toast.message}</p>
			<button type="button" aria-label="Stäng meddelande" on:click={() => toasts.dismiss(toast.id)}>x</button>
		</div>
	{/each}
</div>

<style>
	.toast-region {
		position: fixed;
		right: 1rem;
		bottom: 1rem;
		z-index: 80;
		display: grid;
		gap: 0.75rem;
		width: min(360px, calc(100vw - 2rem));
	}

	.toast {
		display: flex;
		align-items: flex-start;
		justify-content: space-between;
		gap: 1rem;
		padding: 0.95rem 1rem;
		border: 1px solid var(--color-border);
		border-radius: 1rem;
		background: rgba(255, 250, 244, 0.95);
		box-shadow: var(--shadow-soft);
		color: var(--color-text);
	}

	.toast p {
		margin: 0;
	}

	.toast button {
		width: 1.7rem;
		height: 1.7rem;
		padding: 0;
		border: 0;
		border-radius: 999px;
		background: transparent;
		color: var(--color-muted);
		font-weight: 800;
	}

	.toast--success {
		border-color: rgba(143, 162, 132, 0.42);
	}

	.toast--error {
		border-color: rgba(155, 63, 53, 0.34);
	}
</style>
