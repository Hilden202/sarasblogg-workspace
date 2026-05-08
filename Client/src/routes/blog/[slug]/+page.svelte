<script lang="ts">
	import BlogPost from '$lib/components/blog/BlogPost.svelte';
	import LikeButton from '$lib/components/blog/LikeButton.svelte';
	import CommentSection from '$lib/components/comments/CommentSection.svelte';

	export let data;
</script>

<svelte:head>
	<title>{data.post.title} | SarasBlogg</title>
	<meta name="description" content={data.post.title} />
</svelte:head>

<BlogPost post={data.post} />
<LikeButton bloggId={data.post.id} initialLike={data.like} />
<CommentSection bloggId={data.post.id} comments={data.comments} />
