<script lang="ts">
	import type { AdminBlogPostDto, BloggImageDto } from '$lib/types/blog';
	import { formatDate } from '$lib/utils/dates';
	import { resolveMediaUrl } from '$lib/utils/routes';

	export let post: AdminBlogPostDto;
	export let mode: 'published' | 'hidden' = 'published';
	export let busy = false;
	export let canManagePosts = false;
	export let canToggleStatus = false;
	export let onEdit: (post: AdminBlogPostDto) => void = () => {};
	export let onToggleHidden: (post: AdminBlogPostDto) => void | Promise<void> = () => {};
	export let onDelete: (post: AdminBlogPostDto) => void | Promise<void> = () => {};

	type AdminPostWithCover = AdminBlogPostDto & {
		coverImage?: BloggImageDto | null;
		firstImage?: BloggImageDto | null;
	};

	const fallbackImage = resolveMediaUrl('img/blogg/default.png');

	$: image = getImageSource(post);

	function getImageSource(item: AdminPostWithCover) {
		const cover =
			item.coverImage ??
			item.firstImage ??
			[...(item.images ?? [])].sort((a, b) => a.order - b.order || a.id - b.id)[0];

		return cover?.filePath ? resolveMediaUrl(cover.filePath) : fallbackImage;
	}

	function useFallbackImage(event: Event) {
		const imageElement = event.currentTarget as HTMLImageElement;
		if (imageElement.getAttribute('src') === fallbackImage) return;
		imageElement.src = fallbackImage;
	}
</script>

<article class="post-card">
	<div class="post-card__media">
		<img src={image} alt="" loading="lazy" on:error={useFallbackImage} />
	</div>

	<div class="post-card__body">
		<div class="post-card__topline">
			<div>
				<p class="post-card__id">#{post.id}</p>
				<h3>{post.title || 'Utan titel'}</h3>
			</div>
			<div class="post-card__badges" aria-label="Status">
				<span class="badge">{post.hidden ? 'Dold' : 'Synlig'}</span>
				{#if post.isArchived}<span class="badge badge--sage">Arkiv</span>{/if}
			</div>
		</div>

		<dl class="post-card__meta">
			<div>
				<dt>Författare</dt>
				<dd>{post.author || 'SarasBlogg'}</dd>
			</div>
			<div>
				<dt>Publicerad</dt>
				<dd>{formatDate(post.launchDate)}</dd>
			</div>
			<div>
				<dt>Visningar</dt>
				<dd>{post.viewCount}</dd>
			</div>
		</dl>
	</div>

	<div class="post-card__actions" aria-label={`Åtgärder för ${post.title || 'inlägget'}`}>
		{#if mode === 'published'}
			{#if canManagePosts}
				<button type="button" on:click={() => onEdit(post)}>Redigera</button>
			{/if}
			{#if canToggleStatus}
				<button type="button" class="action-warning" disabled={busy} on:click={() => onToggleHidden(post)}>Dölj</button>
			{/if}
		{:else}
			{#if canToggleStatus}
				<button type="button" class="action-success" disabled={busy} on:click={() => onToggleHidden(post)}>Visa</button>
			{/if}
			{#if canManagePosts}
				<button type="button" on:click={() => onEdit(post)}>Redigera</button>
				<button type="button" class="action-danger" disabled={busy} on:click={() => onDelete(post)}>Ta bort</button>
			{/if}
		{/if}
	</div>
</article>

<style>
	.post-card {
		display: grid;
		grid-template-columns: 5.75rem minmax(0, 1fr) auto;
		gap: 1rem;
		align-items: center;
		padding: 1rem;
		border: 1px solid var(--color-border);
		border-radius: 0.75rem;
		background: var(--color-surface);
		box-shadow: var(--shadow-small);
	}

	.post-card__media {
		display: grid;
		place-items: center;
		width: 5.75rem;
		aspect-ratio: 4 / 3;
		overflow: hidden;
		border-radius: 0.65rem;
		background: rgba(244, 217, 202, 0.32);
	}

	.post-card__media img {
		width: 100%;
		height: 100%;
		object-fit: cover;
	}

	.post-card__body {
		display: grid;
		gap: 0.75rem;
		min-width: 0;
	}

	.post-card__topline {
		display: flex;
		flex-wrap: wrap;
		gap: 0.65rem;
		align-items: flex-start;
		justify-content: space-between;
	}

	.post-card__id {
		margin: 0 0 0.12rem;
		color: var(--color-muted);
		font-size: 0.78rem;
		font-weight: 800;
	}

	h3 {
		margin: 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(1.45rem, 3vw, 1.9rem);
		line-height: 1.05;
		overflow-wrap: anywhere;
	}

	.post-card__badges {
		display: flex;
		flex-wrap: wrap;
		gap: 0.4rem;
	}

	.post-card__meta {
		display: flex;
		flex-wrap: wrap;
		gap: 0.8rem 1.25rem;
		margin: 0;
	}

	.post-card__meta div {
		min-width: min(100%, 7rem);
	}

	dt {
		color: var(--color-muted);
		font-size: 0.72rem;
		font-weight: 800;
		letter-spacing: 0.04em;
		text-transform: uppercase;
	}

	dd {
		margin: 0.12rem 0 0;
		color: var(--color-text);
		font-weight: 700;
	}

	.post-card__actions {
		display: flex;
		flex-wrap: wrap;
		justify-content: flex-end;
		gap: 0.45rem;
	}

	button {
		min-height: 2.35rem;
		border: 1px solid var(--color-border);
		border-radius: 999px;
		background: var(--color-surface);
		color: var(--color-heading);
		padding: 0.48rem 0.78rem;
		font-size: 0.84rem;
		font-weight: 800;
	}

	button:disabled {
		cursor: not-allowed;
		opacity: 0.58;
	}

	.action-warning {
		border-color: rgba(217, 155, 121, 0.62);
		color: #8c5b32;
	}

	.action-success {
		border-color: rgba(143, 162, 132, 0.58);
		color: #586b4f;
	}

	.action-danger {
		border-color: rgba(155, 63, 53, 0.32);
		color: #9b3f35;
	}

	@media (max-width: 760px) {
		.post-card {
			grid-template-columns: minmax(0, 1fr);
			gap: 0.85rem;
		}

		.post-card__media {
			width: min(8rem, 42vw);
		}

		.post-card__actions {
			display: grid;
			grid-template-columns: repeat(auto-fit, minmax(8rem, 1fr));
			justify-content: stretch;
		}
	}
</style>
