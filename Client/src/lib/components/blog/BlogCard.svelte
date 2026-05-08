<script lang="ts">
	import type { BlogPostSummaryDto } from '$lib/types/blog';
	import { formatDate, readingMinutes } from '$lib/utils/dates';
	import { blogPostPath, fallbackBlogImage, resolveMediaUrl } from '$lib/utils/routes';

	export let post: BlogPostSummaryDto;
	export let index = 0;

	$: image = post.coverImage?.filePath ? resolveMediaUrl(post.coverImage.filePath) : fallbackBlogImage(index);
	$: title = post.title || 'Utan titel';
</script>

<article class="blog-card">
	<a href={blogPostPath(post)} aria-label={`Läs ${title}`}>
		<div class="blog-card__media">
			<img class="blog-card__blur" src={image} alt="" loading="lazy" aria-hidden="true" />
			<img class="blog-card__image" src={image} alt="" loading="lazy" />
		</div>
		<div class="blog-card__body">
			<p class="blog-card__category">{post.author || 'Reflektioner'}</p>
			<h2>{title}</h2>
			<p>{post.excerpt}</p>
			<footer>
				<span>{formatDate(post.publishedAtUtc)}</span>
				<span>{readingMinutes(post.excerpt)} min läsning</span>
				<i aria-hidden="true">→</i>
			</footer>
		</div>
	</a>
</article>

<style>
	.blog-card {
		overflow: hidden;
		border: 1px solid var(--color-border);
		border-radius: var(--radius-card);
		background: var(--color-surface);
		box-shadow: var(--shadow-small);
		transition:
			transform 170ms ease,
			box-shadow 170ms ease;
	}

	.blog-card:hover {
		transform: translateY(-4px);
		box-shadow: var(--shadow-soft);
	}

	a {
		display: grid;
		grid-template-rows: auto 1fr;
		height: 100%;
	}

	.blog-card__media {
		position: relative;
		display: grid;
		place-items: center;
		width: 100%;
		aspect-ratio: 3 / 2;
		overflow: hidden;
		background: rgba(244, 217, 202, 0.28);
	}

	.blog-card__blur {
		position: absolute;
		inset: 0;
		width: 100%;
		height: 100%;
		object-fit: cover;
		object-position: center;
		filter: blur(18px) brightness(1.04) saturate(1.08);
		transform: scale(1.12);
		opacity: 0.7;
	}

	.blog-card__image {
		position: relative;
		z-index: 1;
		width: 100%;
		height: 100%;
		object-fit: contain;
		object-position: center;
	}

	.blog-card__body {
		display: grid;
		grid-template-rows: auto auto 1fr auto;
		gap: 0.75rem;
		padding: 1.25rem;
	}

	.blog-card__category {
		margin: 0;
		color: var(--color-accent);
		font-size: 0.75rem;
		font-weight: 900;
		letter-spacing: 0.08em;
		text-transform: uppercase;
	}

	h2 {
		margin: 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: 1.55rem;
		line-height: 1.12;
	}

	.blog-card__body > p:not(.blog-card__category) {
		margin: 0;
		color: var(--color-muted);
	}

	footer {
		display: flex;
		align-items: center;
		gap: 0.65rem;
		margin-top: 0.35rem;
		color: var(--color-muted);
		font-size: 0.85rem;
	}

	i {
		display: grid;
		place-items: center;
		width: 2rem;
		height: 2rem;
		margin-left: auto;
		border-radius: 999px;
		background: rgba(244, 217, 202, 0.55);
		color: var(--color-heading);
		font-style: normal;
	}
</style>
