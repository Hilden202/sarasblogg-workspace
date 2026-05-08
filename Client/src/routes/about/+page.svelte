<script lang="ts">
	export let data;
</script>

<svelte:head>
	<title>Om mig | SarasBlogg</title>
	<meta name="description" content="Lär känna Sara bakom SarasBlogg." />
</svelte:head>

<section class="section about-page">
	<div class="container about-page__grid">
		<div class="about-page__image">
			<img src={data.image || data.about?.image || '/images/aboutme/foto.jpg'} alt="" />
		</div>
		<article>
			<p class="eyebrow">Om mig</p>
			<h1>{data.about?.title || 'Med hjärtat som kompass'}</h1>
			<div class="prose">
				{@html data.about?.content || '<p>Här kommer Saras presentation att visas när den finns i API:t.</p>'}
			</div>
		</article>
	</div>
</section>

<style>
	.about-page__grid {
		display: grid;
		grid-template-columns: minmax(260px, 0.8fr) 1.2fr;
		gap: clamp(1.5rem, 5vw, 4rem);
		align-items: start;
	}

	.about-page__image {
		position: sticky;
		top: 8rem;
	}

	img {
		width: 100%;
		aspect-ratio: 0.9;
		border-radius: var(--radius-card);
		object-fit: cover;
		box-shadow: var(--shadow-soft);
	}

	h1 {
		margin: 0.25rem 0 1.25rem;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(3rem, 6vw, 5rem);
		line-height: 0.95;
	}

	@media (max-width: 780px) {
		.about-page__grid {
			grid-template-columns: 1fr;
		}

		.about-page__image {
			position: static;
		}
	}
</style>
