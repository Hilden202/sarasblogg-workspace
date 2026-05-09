<script lang="ts">
	import type { BlogPostDetailDto } from '$lib/types/blog';
	import { formatDate } from '$lib/utils/dates';
	import { fallbackBlogImage, resolveMediaUrl } from '$lib/utils/routes';

	export let post: BlogPostDetailDto;

	let activeIndex = 0;
	let lightboxOpen = false;

	$: gallery = (post.images?.length ? post.images : post.coverImage ? [post.coverImage] : []).map(
		(image, index) => ({
			id: image.id,
			src: resolveMediaUrl(image.filePath),
			alt: `${post.title || 'Bloggbild'} ${index + 1}`
		})
	);
	$: images = gallery.length > 0 ? gallery : [{ id: 0, src: fallbackBlogImage(post.id), alt: '' }];
	$: showTitle = post.showTitle && post.title.trim().length > 0;
	$: readingTime = post.readingTimeMinutes || 1;
	$: if (activeIndex >= images.length) activeIndex = 0;

	function previousImage() {
		activeIndex = activeIndex === 0 ? images.length - 1 : activeIndex - 1;
	}

	function nextImage() {
		activeIndex = activeIndex === images.length - 1 ? 0 : activeIndex + 1;
	}

	function closeLightbox() {
		lightboxOpen = false;
	}

	function handleKeydown(event: KeyboardEvent) {
		if (event.key === 'Escape') {
			closeLightbox();
		}
	}
</script>

<svelte:window on:keydown={handleKeydown} />

<article class="post">
	<header>
		<p class="eyebrow">{post.author || 'SarasBlogg'}</p>
		{#if showTitle}
			<h1>{post.title}</h1>
		{/if}
		<div class="post__meta">
			<span>{formatDate(post.publishedAtUtc)}</span>
			<span>{readingTime} min läsning</span>
			<span>{post.viewCount} visningar</span>
		</div>
	</header>

	<section class="gallery" aria-label="Inläggsbilder">
		<div class="gallery__frame">
			<button
				type="button"
				class="gallery__open"
				aria-label="Visa bilden större"
				on:click={() => (lightboxOpen = true)}
			>
				<img class="post__cover-blur" src={images[activeIndex].src} alt="" aria-hidden="true" />
				<img class="post__cover" src={images[activeIndex].src} alt={images[activeIndex].alt} />
			</button>
			{#if images.length > 1}
				<button
					type="button"
					class="gallery__nav gallery__nav--prev"
					aria-label="Föregående bild"
					on:click|stopPropagation={previousImage}>‹</button
				>
				<button
					type="button"
					class="gallery__nav gallery__nav--next"
					aria-label="Nästa bild"
					on:click|stopPropagation={nextImage}>›</button
				>
			{/if}
		</div>
		{#if images.length > 1}
			<div class="gallery__thumbs" aria-label="Bildminiatyrer">
				{#each images as image, index (image.id || image.src)}
					<button
						type="button"
						class:active={index === activeIndex}
						aria-label={`Visa bild ${index + 1}`}
						aria-current={index === activeIndex}
						on:click={() => (activeIndex = index)}
					>
						<img src={image.src} alt="" loading="lazy" />
					</button>
				{/each}
			</div>
		{/if}
	</section>

	<div class="post__content prose">
		<!-- eslint-disable-next-line svelte/no-at-html-tags -->
		{@html post.content}
	</div>
</article>

{#if lightboxOpen}
	<div
		class="lightbox"
		role="dialog"
		aria-modal="true"
		aria-label="Förstorad bloggbild"
		tabindex="-1"
	>
		<button
			type="button"
			class="lightbox__backdrop"
			aria-label="Stäng bild"
			on:click={closeLightbox}
		></button>
		<div class="lightbox__content">
			<button type="button" aria-label="Stäng bild" on:click={closeLightbox}>×</button>
			<img src={images[activeIndex].src} alt={images[activeIndex].alt} />
		</div>
	</div>
{/if}

<style>
	.post {
		padding-top: clamp(2rem, 5vw, 4rem);
	}

	header {
		width: min(100%, 820px);
		margin-inline: auto;
		text-align: center;
	}

	h1 {
		margin: 0.35rem 0 1rem;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(2.7rem, 6vw, 5.4rem);
		font-weight: 600;
		line-height: 0.98;
	}

	.post__meta {
		display: flex;
		flex-wrap: wrap;
		justify-content: center;
		gap: 0.65rem 1rem;
		color: var(--color-muted);
	}

	.gallery {
		width: min(100% - 2rem, 980px);
		margin: 2rem auto;
	}

	.gallery__frame {
		position: relative;
	}

	.gallery__open {
		position: relative;
		display: block;
		width: 100%;
		aspect-ratio: 3 / 2;
		overflow: hidden;
		padding: 0;
		border: 0;
		border-radius: var(--radius-card);
		background: rgba(244, 217, 202, 0.28);
		box-shadow: var(--shadow-soft);
	}

	.post__cover-blur {
		position: absolute;
		inset: 0;
		width: 100%;
		height: 100%;
		object-fit: cover;
		object-position: center;
		filter: blur(22px) brightness(1.04) saturate(1.06);
		transform: scale(1.12);
		opacity: 0.72;
	}

	.post__cover {
		position: relative;
		z-index: 1;
		width: 100%;
		height: 100%;
		object-fit: contain;
		object-position: center;
		cursor: zoom-in;
	}

	.gallery__nav {
		position: absolute;
		top: 50%;
		z-index: 2;
		width: 2.75rem;
		height: 2.75rem;
		border: 1px solid rgba(255, 250, 244, 0.82);
		border-radius: 999px;
		background: rgba(95, 74, 59, 0.62);
		color: #fffaf4;
		font-size: 2rem;
		line-height: 1;
		transform: translateY(-50%);
	}

	.gallery__nav--prev {
		left: 1rem;
	}

	.gallery__nav--next {
		right: 1rem;
	}

	.gallery__thumbs {
		display: flex;
		flex-wrap: wrap;
		justify-content: center;
		gap: 0.55rem;
		margin-top: 0.75rem;
	}

	.gallery__thumbs button {
		width: 4.2rem;
		aspect-ratio: 1;
		padding: 0;
		border: 2px solid transparent;
		border-radius: 0.85rem;
		background: transparent;
		overflow: hidden;
		opacity: 0.68;
	}

	.gallery__thumbs button.active {
		border-color: var(--color-accent);
		opacity: 1;
	}

	.gallery__thumbs img {
		width: 100%;
		height: 100%;
		object-fit: cover;
	}

	.post__content {
		width: min(100% - 2rem, 760px);
		margin-inline: auto;
		padding-bottom: 2rem;
		font-size: 1.08rem;
	}

	.lightbox {
		position: fixed;
		inset: 0;
		z-index: 85;
		display: grid;
		place-items: center;
		padding: 2rem;
		background: rgba(37, 28, 21, 0.86);
	}

	.lightbox__backdrop {
		position: absolute;
		inset: 0;
		width: 100%;
		height: 100%;
		border: 0;
		background: transparent;
	}

	.lightbox__content {
		position: relative;
		z-index: 1;
		display: grid;
		place-items: center;
		max-width: min(100%, 1120px);
		max-height: 88vh;
	}

	.lightbox img {
		max-width: 100%;
		max-height: 88vh;
		border-radius: var(--radius-soft);
		object-fit: contain;
		box-shadow: var(--shadow-soft);
	}

	.lightbox__content > button {
		position: fixed;
		top: 1rem;
		right: 1rem;
		width: 2.75rem;
		height: 2.75rem;
		border: 1px solid rgba(255, 250, 244, 0.45);
		border-radius: 999px;
		background: rgba(255, 250, 244, 0.13);
		color: #fffaf4;
		font-size: 1.6rem;
	}

	@media (max-width: 640px) {
		.gallery__nav {
			width: 2.35rem;
			height: 2.35rem;
			font-size: 1.55rem;
		}

		.gallery__thumbs button {
			width: 3.35rem;
		}
	}
</style>
