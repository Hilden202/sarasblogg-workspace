<script lang="ts">
	export let href: string | null = null;
	export let type: 'button' | 'submit' | 'reset' = 'button';
	export let variant: 'primary' | 'secondary' | 'ghost' | 'danger' = 'primary';
	export let disabled = false;
	export let full = false;
	export let ariaLabel: string | undefined = undefined;
</script>

{#if href}
	<a class:button--full={full} class="button button--{variant}" {href} aria-label={ariaLabel}>
		<slot />
	</a>
{:else}
	<button class:button--full={full} class="button button--{variant}" {type} {disabled} aria-label={ariaLabel} on:click>
		<slot />
	</button>
{/if}

<style>
	.button {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		gap: 0.5rem;
		min-height: 2.7rem;
		padding: 0.72rem 1.15rem;
		border: 1px solid transparent;
		border-radius: 999px;
		font-size: 0.88rem;
		font-weight: 800;
		letter-spacing: 0.04em;
		line-height: 1;
		text-transform: uppercase;
		transition:
			transform 160ms ease,
			box-shadow 160ms ease,
			background-color 160ms ease,
			border-color 160ms ease;
	}

	.button:hover {
		transform: translateY(-1px);
	}

	.button:disabled {
		cursor: not-allowed;
		opacity: 0.58;
		transform: none;
	}

	.button--full {
		width: 100%;
	}

	.button--primary {
		background: #9b6a38;
		color: #fffaf4;
		box-shadow: 0 12px 26px rgba(111, 79, 44, 0.2);
	}

	.button--primary:hover {
		background: #85582e;
	}

	.button--secondary {
		background: var(--color-surface);
		border-color: rgba(217, 155, 121, 0.55);
		color: var(--color-heading);
	}

	.button--ghost {
		background: transparent;
		border-color: transparent;
		color: var(--color-muted);
	}

	.button--ghost:hover {
		background: rgba(255, 250, 244, 0.72);
		color: var(--color-heading);
	}

	.button--danger {
		background: #9b3f35;
		color: #fffaf4;
	}
</style>
