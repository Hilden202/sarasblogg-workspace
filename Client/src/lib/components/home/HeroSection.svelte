<script lang="ts">
	import Button from '$lib/components/ui/Button.svelte';
	import BrandLockup from '$lib/components/brand/BrandLockup.svelte';
	import SpotifyCard from './SpotifyCard.svelte';
	import { routes } from '$lib/utils/routes';
</script>

<section class="hero">
	<div class="container hero__inner">
		<div class="hero__brand">
			<BrandLockup variant="hero" heading />
		</div>
		<div class="hero__copy" aria-label="Introduktion">
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
		padding: clamp(2rem, 4.5vw, 4rem) 0 2.75rem;
	}

	.hero__inner {
		display: grid;
		grid-template-columns: minmax(0, 0.78fr) minmax(0, 760px) minmax(0, 1.22fr);
		justify-items: center;
		align-items: center;
		gap: clamp(1.75rem, 3.6vw, 2.85rem);
		text-align: center;
	}

	.hero__brand {
		grid-column: 2;
		display: flex;
		justify-content: center;
		width: 100%;
		filter: drop-shadow(0 18px 28px rgba(95, 74, 59, 0.08));
	}

	.hero__copy {
		grid-column: 2;
		min-width: 0;
		display: grid;
		justify-items: center;
		gap: clamp(0.35rem, 1vw, 0.75rem);
		margin-top: 0.15rem;
	}

	.hero__actions {
		display: flex;
		flex-wrap: wrap;
		gap: 0.75rem;
		justify-content: center;
	}

	.hero :global(.spotify-card) {
		grid-column: 2;
		width: min(100%, 760px);
	}

	@media (max-width: 760px) {
		.hero {
			padding-top: 1.6rem;
		}

		.hero__inner {
			grid-template-columns: 1fr;
		}

		.hero__brand,
		.hero__copy,
		.hero :global(.spotify-card) {
			grid-column: 1;
		}

		.hero__actions {
			justify-content: center;
		}
	}
</style>
