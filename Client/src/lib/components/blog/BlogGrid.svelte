<script lang="ts">
	import type { BlogPostSummaryDto } from '$lib/types/blog';
	import BlogCard from './BlogCard.svelte';
	import EmptyState from '$lib/components/ui/EmptyState.svelte';

	export let posts: BlogPostSummaryDto[] = [];
</script>

{#if posts.length > 0}
	<div class="blog-grid">
		{#each posts as post, index (post.id)}
			<BlogCard {post} {index} />
		{/each}
	</div>
{:else}
	<EmptyState title="Inga inlägg ännu" text="När nya texter publiceras samlas de här." />
{/if}

<style>
	.blog-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(min(100%, 18rem), 1fr));
		gap: clamp(1rem, 3vw, 1.55rem);
	}
</style>
