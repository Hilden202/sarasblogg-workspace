<script lang="ts">
	import { brand } from '$lib/config/site';

	export let title: string;
	export let text = '';
</script>

<section class="auth-panel">
	<div class="auth-panel__brand">
		<img src={brand.compactLogo} alt="" />
	</div>
	<div class="auth-panel__content">
		<h1>{title}</h1>
		{#if text}
			<p>{text}</p>
		{/if}
		<slot />
	</div>
</section>

<style>
	.auth-panel {
		display: grid;
		grid-template-columns: 0.8fr 1fr;
		gap: clamp(1.25rem, 4vw, 3rem);
		align-items: center;
		width: min(100% - 2rem, 920px);
		margin: clamp(2rem, 6vw, 5rem) auto;
		padding: clamp(1.25rem, 4vw, 2.25rem);
		border: 1px solid var(--color-border);
		border-radius: calc(var(--radius-card) + 0.5rem);
		background: rgba(255, 250, 244, 0.82);
		box-shadow: var(--shadow-soft);
	}

	.auth-panel__brand {
		display: grid;
		place-items: center;
		min-height: 100%;
		border-radius: var(--radius-card);
		background: rgba(244, 217, 202, 0.38);
	}

	.auth-panel__brand img {
		width: min(230px, 70%);
	}

	.auth-panel__content {
		display: grid;
		gap: 1rem;
	}

	h1 {
		margin: 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(2.4rem, 5vw, 4.2rem);
		line-height: 0.95;
	}

	p {
		margin: 0;
		color: var(--color-muted);
	}

	@media (max-width: 760px) {
		.auth-panel {
			grid-template-columns: 1fr;
		}

		.auth-panel__brand {
			min-height: 160px;
		}
	}
</style>
