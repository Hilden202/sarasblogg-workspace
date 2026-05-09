<script lang="ts">
	import { resolveMediaUrl, staticAsset } from '$lib/utils/routes';

	export let data;

	const aboutFallbackImage = staticAsset('/images/aboutme/foto.jpg');

	$: aboutImage = data.image || data.about?.image ? resolveMediaUrl(data.image || data.about?.image) : aboutFallbackImage;
</script>

<svelte:head>
	<title>Om mig | SarasBlogg</title>
	<meta name="description" content="Lär känna Sara bakom SarasBlogg." />
</svelte:head>

<section class="section about-page">
	<div class="container about-page__grid">
		<div class="about-page__image">
			<img src={aboutImage} alt="" />
		</div>
		<article class="about-page__content">
			<p class="eyebrow">Om mig</p>
			<h1>{data.about?.title || 'Med hjärtat som kompass'}</h1>
			<div class="prose about-page__prose">
				{@html data.about?.content ||
					'<p>Här kommer Saras presentation att visas när den finns i API:t.</p>'}
			</div>
		</article>
	</div>
</section>

<style>
	.about-page__grid {
		display: grid;
		grid-template-columns: minmax(220px, 320px) minmax(0, 620px);
		gap: clamp(2rem, 4vw, 3.25rem);
		align-items: start;
		justify-content: start;
		width: min(100% - 2rem, 1040px);
		max-width: none;
		margin-inline: auto;
	}

	.about-page__image {
		top: 8rem;
		justify-self: start;
		width: 100%;
		max-width: 320px;
	}

	img {
		width: 100%;
		aspect-ratio: 0.82;
		border-radius: var(--radius-card);
		object-fit: cover;
		box-shadow: var(--shadow-soft);
	}

	.about-page__content {
		justify-self: start;
		width: min(100%, 620px);
	}

	h1 {
		margin: 0.25rem 0 1.25rem;
		max-width: 11ch;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(2.75rem, 4.8vw, 4.5rem);
		line-height: 0.95;
	}

	.about-page__prose {
		max-width: 62ch;
		line-height: 1.76;
	}

	.about-page__prose :global(p),
	.about-page__prose :global(div),
	.about-page__prose :global(li),
	.about-page__prose :global(span) {
		max-width: 62ch;
	}

	.about-page__prose :global(h1),
	.about-page__prose :global(h2),
	.about-page__prose :global(h3),
	.about-page__prose :global(h4),
	.about-page__prose :global(h5),
	.about-page__prose :global(h6) {
		max-width: 16ch;
	}

	.about-page__prose :global(img) {
		max-width: min(100%, 34rem);
		height: auto;
	}

	@media (max-width: 780px) {
		.about-page__grid {
			grid-template-columns: 1fr;
			width: min(100% - 2rem, 620px);
		}

		.about-page__image {
			position: static;
			max-width: 360px;
		}
	}
</style>
