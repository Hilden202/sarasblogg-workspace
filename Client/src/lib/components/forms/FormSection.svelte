<script lang="ts">
	export let title: string;
	export let text = '';
</script>

<section class="form-section">
	<header>
		<h2>{title}</h2>
		{#if text}
			<p>{text}</p>
		{/if}
	</header>
	<slot />
</section>

<style>
	.form-section {
		padding: clamp(1.25rem, 3vw, 2rem);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-card);
		background: var(--color-surface-glass);
		box-shadow: var(--shadow-small);
	}

	header {
		margin-bottom: 1rem;
	}

	h2 {
		margin: 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(1.7rem, 3vw, 2.3rem);
	}

	p {
		margin: 0.35rem 0 0;
		color: var(--color-muted);
	}
</style>
