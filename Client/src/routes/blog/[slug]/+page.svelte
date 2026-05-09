<script lang="ts">
	import BlogPost from '$lib/components/blog/BlogPost.svelte';
	import LikeButton from '$lib/components/blog/LikeButton.svelte';
	import CommentSection from '$lib/components/comments/CommentSection.svelte';

	export let data;

	$: pageTitle = data.post.title || 'Blogginlägg';
</script>

<svelte:head>
	<title>{pageTitle} | SarasBlogg</title>
	<meta name="description" content={pageTitle} />
</svelte:head>

<BlogPost post={data.post} />
<LikeButton bloggId={data.post.id} initialLike={data.like} />
<CommentSection bloggId={data.post.id} comments={data.comments} />
