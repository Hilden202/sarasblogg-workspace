<script lang="ts">
	import Button from '$lib/components/ui/Button.svelte';
	import SpotifyCard from './SpotifyCard.svelte';
	import { brand } from '$lib/config/site';
	import { routes } from '$lib/utils/routes';
</script>

<section class="hero">
	<div class="container hero__inner">
		<div class="hero__brand">
			<h1>{brand.name}</h1>
			<img src={brand.heroLogo} alt={brand.name} />
		</div>
		<div class="hero__copy" aria-label="Introduktion">
			<p>{brand.tagline}</p>
			<div class="hero__actions">
				<Button href={routes.blog}>Till bloggen</Button>
				<Button href={routes.about} variant="secondary">Om mig</Button>
			</div>
		</div>
		<SpotifyCard />
	</div>
</section>

<style>
	.hero {
		padding: clamp(2rem, 5vw, 4rem) 0 2.25rem;
	}

	.hero__inner {
		display: grid;
		justify-items: center;
		align-items: center;
		gap: clamp(1rem, 3vw, 1.75rem);
		text-align: center;
	}

	.hero__brand {
		display: grid;
		justify-items: center;
		width: min(100%, 760px);
	}

	h1 {
		position: absolute;
		width: 1px;
		height: 1px;
		padding: 0;
		margin: -1px;
		overflow: hidden;
		clip: rect(0, 0, 0, 0);
		white-space: nowrap;
		border: 0;
	}

	.hero__brand img {
		width: min(100%, 700px);
		height: clamp(250px, 42vw, 430px);
		object-fit: contain;
		filter: drop-shadow(0 18px 28px rgba(95, 74, 59, 0.1));
	}

	.hero__copy {
		min-width: 0;
		display: grid;
		justify-items: center;
	}

	.hero__copy > p:not(.eyebrow) {
		max-width: 34rem;
		margin: 0;
		color: var(--color-muted);
		font-family: var(--font-serif);
		font-size: clamp(1.15rem, 2vw, 1.45rem);
		line-height: 1.35;
	}

	.hero__actions {
		display: flex;
		flex-wrap: wrap;
		gap: 0.75rem;
		margin-top: 1.5rem;
	}

	.hero :global(.spotify-card) {
		width: min(100%, 760px);
	}

	@media (max-width: 760px) {
		.hero {
			padding-top: 1.75rem;
		}

		.hero__brand img {
			height: clamp(220px, 68vw, 320px);
		}

		.hero__copy > p:not(.eyebrow) {
			margin-inline: auto;
		}

		.hero__actions {
			justify-content: center;
		}
	}
</style>
