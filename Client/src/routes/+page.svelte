<script lang="ts">
	import HeroSection from '$lib/components/home/HeroSection.svelte';
	import LatestPosts from '$lib/components/blog/LatestPosts.svelte';
	import { brand } from '$lib/config/site';

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
