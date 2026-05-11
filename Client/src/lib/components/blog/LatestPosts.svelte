<script lang="ts">
	import Button from '$lib/components/ui/Button.svelte';
	import type { BlogPostSummaryDto } from '$lib/types/blog';
	import { routes, staticAsset } from '$lib/utils/routes';
	import BlogGrid from './BlogGrid.svelte';

	export let posts: BlogPostSummaryDto[] = [];

	const sectionDivider = staticAsset('/images/logo/section-divider-heart.png');
</script>

<section class="section section--surface latest-posts">
	<div class="container">
		<h2 class="section-title latest-posts__title">Senaste inläggen</h2>
		<img
			class="latest-posts__divider"
			src={sectionDivider}
			alt=""
			width="881"
			height="83"
			aria-hidden="true"
			loading="lazy"
		/>
		<BlogGrid {posts} />
		<div class="latest-posts__action">
			<Button href={routes.blog} variant="secondary">Till bloggen</Button>
		</div>
	</div>
</section>

<style>
	.latest-posts__title {
		margin-bottom: clamp(0.7rem, 2vw, 1rem);
	}

	.latest-posts__divider {
		width: clamp(170px, 34vw, 330px);
		height: auto;
		margin: 0 auto clamp(1.35rem, 3.5vw, 2.35rem);
		opacity: 0.78;
	}

	.latest-posts__action {
		display: flex;
		justify-content: center;
		margin-top: 2rem;
	}
</style>
