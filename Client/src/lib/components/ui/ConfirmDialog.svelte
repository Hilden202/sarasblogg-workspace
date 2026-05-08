<script lang="ts">
	import { tick } from 'svelte';
	import { confirmDialog } from '$lib/stores/confirmStore';
	import Button from './Button.svelte';

	let confirmButton: HTMLButtonElement | null = null;

	$: if ($confirmDialog) {
		tick().then(() => confirmButton?.focus());
	}

	function close(confirmed: boolean) {
		confirmDialog.answer(confirmed);
	}

	function handleKeydown(event: KeyboardEvent) {
		if ($confirmDialog && event.key === 'Escape') {
			event.preventDefault();
			close(false);
		}
	}
</script>

<svelte:window on:keydown={handleKeydown} />

{#if $confirmDialog}
	<div class="confirm-backdrop" role="presentation">
		<div
			class="confirm"
			class:confirm--danger={$confirmDialog.tone === 'danger'}
			role="dialog"
			aria-modal="true"
			aria-labelledby="confirm-title"
			aria-describedby="confirm-message"
			tabindex="-1"
		>
			<h2 id="confirm-title">{$confirmDialog.title}</h2>
			<p id="confirm-message">{$confirmDialog.message}</p>
			<div class="confirm__actions">
				<Button variant="ghost" on:click={() => close(false)}>{$confirmDialog.cancelLabel}</Button>
				<button
					bind:this={confirmButton}
					type="button"
					class="confirm__primary"
					class:confirm__primary--danger={$confirmDialog.tone === 'danger'}
					on:click={() => close(true)}
				>
					{$confirmDialog.confirmLabel}
				</button>
			</div>
		</div>
	</div>
{/if}

<style>
	.confirm-backdrop {
		position: fixed;
		inset: 0;
		z-index: 90;
		display: grid;
		place-items: center;
		padding: 1rem;
		background: rgba(72, 54, 40, 0.28);
		backdrop-filter: blur(5px);
	}

	.confirm {
		width: min(100%, 440px);
		padding: 1.35rem;
		border: 1px solid var(--color-border);
		border-radius: var(--radius-card);
		background: var(--color-surface);
		box-shadow: var(--shadow-soft);
	}

	h2 {
		margin: 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: 2rem;
		line-height: 1.05;
	}

	p {
		margin: 0.75rem 0 0;
		color: var(--color-muted);
	}

	.confirm__actions {
		display: flex;
		flex-wrap: wrap;
		justify-content: flex-end;
		gap: 0.65rem;
		margin-top: 1.25rem;
	}

	.confirm__primary {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-height: 2.7rem;
		padding: 0.72rem 1.15rem;
		border: 1px solid transparent;
		border-radius: 999px;
		background: #9b6a38;
		color: #fffaf4;
		font-size: 0.88rem;
		font-weight: 800;
		letter-spacing: 0.04em;
		line-height: 1;
		text-transform: uppercase;
		box-shadow: 0 12px 26px rgba(111, 79, 44, 0.2);
	}

	.confirm__primary--danger {
		background: #9b3f35;
	}
</style>
