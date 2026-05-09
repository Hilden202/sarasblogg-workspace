<script lang="ts">
	import type { BlogPostSummaryDto } from '$lib/types/blog';
	import { formatDate, readingMinutes } from '$lib/utils/dates';
	import { blogPostPath, fallbackBlogImage, resolveMediaUrl } from '$lib/utils/routes';

	export let post: BlogPostSummaryDto;
	export let index = 0;
	export let variant: 'compact' | 'editorial' = 'compact';

	$: image = post.coverImage?.filePath
		? resolveMediaUrl(post.coverImage.filePath)
		: fallbackBlogImage(index);
	$: title = post.title || 'Utan titel';
</script>

<article class="blog-card" class:blog-card--editorial={variant === 'editorial'}>
	<a href={blogPostPath(post)} aria-label={`Läs ${title}`}>
		<div class="blog-card__media">
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
		width: 100%;
		max-width: 18rem;
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
		transform: translateY(-3px);
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
		height: clamp(11rem, 21vw, 13rem);
		overflow: hidden;
		background: rgba(255, 250, 244, 0.72);
	}

	.blog-card--editorial {
		max-width: 32rem;
	}

	.blog-card--editorial .blog-card__media {
		height: clamp(15rem, 32vw, 22rem);
	}

	.blog-card--editorial .blog-card__body {
		gap: 0.75rem;
		padding: clamp(1.2rem, 2.5vw, 1.65rem);
	}

	.blog-card--editorial h2 {
		font-size: clamp(1.45rem, 2.4vw, 1.85rem);
		line-height: 1.12;
	}

	.blog-card--editorial .blog-card__body > p:not(.blog-card__category) {
		-webkit-line-clamp: 3;
		line-clamp: 3;
		font-size: 0.98rem;
		line-height: 1.58;
	}

	.blog-card--editorial footer {
		margin-top: 0.35rem;
		font-size: 0.8rem;
	}

	.blog-card__image {
		width: 100%;
		height: 100%;
		object-fit: contain;
		object-position: center;
	}

	.blog-card__body {
		display: grid;
		grid-template-rows: auto auto 1fr auto;
		gap: 0.58rem;
		padding: 1rem 1.08rem 1.08rem;
	}

	.blog-card__category {
		margin: 0;
		color: var(--color-accent);
		font-size: 0.66rem;
		font-weight: 900;
		letter-spacing: 0.08em;
		text-transform: uppercase;
	}

	h2 {
		margin: 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: 1.28rem;
		line-height: 1.16;
	}

	.blog-card__body > p:not(.blog-card__category) {
		display: -webkit-box;
		-webkit-box-orient: vertical;
		-webkit-line-clamp: 2;
		line-clamp: 2;
		overflow: hidden;
		margin: 0;
		color: var(--color-muted);
		font-size: 0.86rem;
		line-height: 1.5;
	}

	footer {
		display: flex;
		align-items: center;
		gap: 0.45rem;
		margin-top: 0.2rem;
		color: var(--color-muted);
		font-size: 0.72rem;
	}

	i {
		display: grid;
		place-items: center;
		width: 1.55rem;
		height: 1.55rem;
		margin-left: auto;
		border-radius: 999px;
		background: rgba(244, 217, 202, 0.55);
		color: var(--color-heading);
		font-style: normal;
	}

	@media (max-width: 640px) {
		.blog-card--editorial .blog-card__media {
			height: clamp(13rem, 62vw, 17rem);
		}
	}
</style>
