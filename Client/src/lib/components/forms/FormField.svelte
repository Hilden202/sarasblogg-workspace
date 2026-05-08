<script lang="ts">
	export let label: string;
	export let id: string;
	export let error: string | null = null;
	export let hint: string | null = null;
</script>

<label class="field" for={id}>
	<span>{label}</span>
	<slot />
	{#if hint}
		<small>{hint}</small>
	{/if}
	{#if error}
		<strong>{error}</strong>
	{/if}
</label>

<style>
	.field {
		display: grid;
		gap: 0.4rem;
		color: var(--color-heading);
		font-weight: 700;
	}

	.field :global(input),
	.field :global(textarea),
	.field :global(select) {
		width: 100%;
		border: 1px solid var(--color-border);
		border-radius: 0.9rem;
		background: rgba(255, 250, 244, 0.9);
		color: var(--color-text);
		padding: 0.78rem 0.95rem;
		box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.55);
	}

	.field :global(textarea) {
		min-height: 9rem;
		resize: vertical;
	}

	small {
		color: var(--color-muted);
		font-weight: 500;
	}

	strong {
		color: #9b3f35;
		font-size: 0.9rem;
	}
</style>
