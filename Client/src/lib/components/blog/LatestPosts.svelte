<script lang="ts">
	import Button from '$lib/components/ui/Button.svelte';
	import type { BlogPostSummaryDto } from '$lib/types/blog';
	import BlogGrid from './BlogGrid.svelte';

	export let posts: BlogPostSummaryDto[] = [];
</script>

<section class="section section--surface latest-posts">
	<div class="container">
		<h2 class="section-title">Senaste inläggen</h2>
		<p class="section-lead">Nya texter om vardagsmod, tacksamhet och att lyssna inåt.</p>
		<BlogGrid {posts} />
		<div class="latest-posts__action">
			<Button href="/blog" variant="secondary">Till bloggen</Button>
		</div>
	</div>
</section>

<style>
	.latest-posts__action {
		display: flex;
		justify-content: center;
		margin-top: 2rem;
	}
</style>
