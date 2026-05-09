<script lang="ts">
	import Button from '$lib/components/ui/Button.svelte';
	import type { BlogPostSummaryDto } from '$lib/types/blog';
	import { routes } from '$lib/utils/routes';
	import BlogGrid from './BlogGrid.svelte';

	export let posts: BlogPostSummaryDto[] = [];
</script>

<section class="section section--surface latest-posts">
	<div class="container">
		<h2 class="section-title latest-posts__title">Senaste inläggen</h2>
		<BlogGrid {posts} />
		<div class="latest-posts__action">
			<Button href={routes.blog} variant="secondary">Till bloggen</Button>
		</div>
	</div>
</section>

<style>
	.latest-posts__title {
		margin-bottom: clamp(1.75rem, 4vw, 2.5rem);
	}

	.latest-posts__action {
		display: flex;
		justify-content: center;
		margin-top: 2rem;
	}
</style>
