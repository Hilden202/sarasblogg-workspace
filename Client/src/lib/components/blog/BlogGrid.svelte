<script lang="ts">
	import type { BlogPostSummaryDto } from '$lib/types/blog';
	import BlogCard from './BlogCard.svelte';
	import EmptyState from '$lib/components/ui/EmptyState.svelte';

	export let posts: BlogPostSummaryDto[] = [];
	export let variant: 'compact' | 'editorial' = 'compact';
</script>

{#if posts.length > 0}
	<div class="blog-grid" class:blog-grid--editorial={variant === 'editorial'}>
		{#each posts as post, index (post.id)}
			<BlogCard {post} {index} {variant} />
		{/each}
	</div>
{:else}
	<EmptyState title="Inga inlägg ännu" text="När nya texter publiceras samlas de här." />
{/if}

<style>
	.blog-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(min(100%, 15rem), 18rem));
		justify-content: center;
		gap: clamp(1.1rem, 2.8vw, 1.65rem);
	}

	.blog-grid--editorial {
		grid-template-columns: repeat(auto-fit, minmax(min(100%, 19rem), 32rem));
		gap: clamp(1.5rem, 3.4vw, 2.5rem);
	}
</style>
