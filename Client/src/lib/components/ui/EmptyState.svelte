<script lang="ts">
	export let title = 'Inget att visa ännu';
	export let text = 'Kom tillbaka om en stund.';
</script>

<div class="empty-state">
	<div class="empty-state__mark" aria-hidden="true"></div>
	<h2>{title}</h2>
	<p>{text}</p>
	<slot />
</div>

<style>
	.empty-state {
		padding: clamp(2rem, 6vw, 4rem);
		border: 1px dashed rgba(217, 155, 121, 0.55);
		border-radius: var(--radius-card);
		background: rgba(255, 250, 244, 0.72);
		text-align: center;
	}

	.empty-state__mark {
		width: 3rem;
		height: 3rem;
		margin: 0 auto 1rem;
		border: 1px solid rgba(216, 181, 109, 0.7);
		border-radius: 999px;
		background: rgba(244, 217, 202, 0.45);
	}

	h2 {
		margin: 0 0 0.4rem;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: 2rem;
	}

	p {
		margin: 0 auto 1rem;
		max-width: 34rem;
		color: var(--color-muted);
	}
</style>
