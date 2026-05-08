<script lang="ts">
	import HeroSection from '$lib/components/home/HeroSection.svelte';
	import LatestPosts from '$lib/components/blog/LatestPosts.svelte';
	import Button from '$lib/components/ui/Button.svelte';
	import { brand } from '$lib/config/site';
	import { resolveMediaUrl, routes } from '$lib/utils/routes';
	import { truncate } from '$lib/utils/text';

	export let data;
</script>

<svelte:head>
	<title>SarasBlogg | {brand.name}</title>
	<meta
		name="description"
		content={brand.tagline}
	/>
</svelte:head>

<HeroSection />

<LatestPosts posts={data.latestPosts} />

<section class="section about-preview">
	<div class="container about-preview__grid">
		<div class="about-preview__media">
			<img class="about-preview__blur" src={resolveMediaUrl(data.about?.image || '/images/aboutme/foto.jpg')} alt="" aria-hidden="true" />
			<img class="about-preview__image" src={resolveMediaUrl(data.about?.image || '/images/aboutme/foto.jpg')} alt="" />
		</div>
		<div>
			<p class="eyebrow">Om mig</p>
			<h2>{data.about?.title || 'En plats för det som känns sant'}</h2>
			<p>{truncate(data.about?.content, 260) || 'Här samlas tankar om livet, hjärtats riktning och de mjuka pauserna mellan allt som händer.'}</p>
			<Button href={routes.about} variant="secondary">Läs mer</Button>
		</div>
	</div>
</section>

<section class="section newsletter-band">
	<div class="container newsletter-band__inner">
		<div>
			<p class="eyebrow">Nyhetsbrev</p>
			<h2>Få nya inlägg när de publiceras</h2>
			<p>En stillsam påminnelse i inkorgen när nya texter finns att läsa.</p>
		</div>
		<form on:submit|preventDefault>
			<input type="email" placeholder="Din e-postadress" aria-label="Din e-postadress" />
			<Button type="submit">Anmäl</Button>
		</form>
	</div>
</section>

<style>
	.about-preview__grid,
	.newsletter-band__inner {
		display: grid;
		grid-template-columns: 0.95fr 1.05fr;
		gap: clamp(1.5rem, 5vw, 4rem);
		align-items: center;
	}

	.about-preview__media {
		position: relative;
		display: grid;
		place-items: center;
		width: 100%;
		aspect-ratio: 1;
		overflow: hidden;
		border-radius: var(--radius-card);
		background: rgba(244, 217, 202, 0.28);
		box-shadow: var(--shadow-soft);
	}

	.about-preview__blur {
		position: absolute;
		inset: 0;
		width: 100%;
		height: 100%;
		object-fit: cover;
		filter: blur(20px) brightness(1.03) saturate(1.05);
		transform: scale(1.1);
		opacity: 0.62;
	}

	.about-preview__image {
		position: relative;
		z-index: 1;
		width: 100%;
		height: 100%;
		object-fit: cover;
		object-position: center;
	}

	.about-preview h2,
	.newsletter-band h2 {
		margin: 0.25rem 0 1rem;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(2.2rem, 5vw, 4rem);
		line-height: 1;
	}

	.about-preview p:not(.eyebrow),
	.newsletter-band p:not(.eyebrow) {
		max-width: 36rem;
		margin: 0 0 1.25rem;
		color: var(--color-muted);
	}

	.newsletter-band {
		padding-top: 1rem;
	}

	.newsletter-band__inner {
		padding: clamp(1.5rem, 4vw, 2.4rem);
		border: 1px solid var(--color-border);
		border-radius: var(--radius-card);
		background: rgba(255, 250, 244, 0.78);
		box-shadow: var(--shadow-small);
	}

	.newsletter-band form {
		display: grid;
		grid-template-columns: 1fr auto;
		gap: 0.75rem;
	}

	.newsletter-band input {
		min-width: 0;
		border: 1px solid var(--color-border);
		border-radius: 999px;
		background: #fff;
		padding: 0.85rem 1rem;
	}

	@media (max-width: 760px) {
		.about-preview__grid,
		.newsletter-band__inner,
		.newsletter-band form {
			grid-template-columns: 1fr;
		}
	}
</style>
