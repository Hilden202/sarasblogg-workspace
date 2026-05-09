<script lang="ts">
	import BlogPostPage from '../../blog/[slug]/+page.svelte';

	export let data;
</script>

<BlogPostPage {data} />
