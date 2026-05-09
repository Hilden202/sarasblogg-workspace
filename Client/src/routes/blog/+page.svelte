<script lang="ts">
	import BlogGrid from '$lib/components/blog/BlogGrid.svelte';
	import Button from '$lib/components/ui/Button.svelte';

	export let data;
</script>

<svelte:head>
	<title>Blogg | SarasBlogg</title>
	<meta name="description" content="Läs de senaste inläggen från SarasBlogg." />
</svelte:head>

<section class="section blog-index">
	<div class="container">
		<header>
			<img
				class="blog-index__logo"
				src="/images/logo/sarablogglogga.png"
				alt="SarasBlogg textlogga"
			/>
			<div class="filters">
				<Button href="/blog" variant={!data.archive ? 'primary' : 'secondary'}>Aktuellt</Button>
				<Button href="/blog?archive=true" variant={data.archive ? 'primary' : 'secondary'}>Arkiv</Button>
			</div>
		</header>

		{#if data.error}
			<p class="status-text status-text--error">{data.error}</p>
		{/if}

		<BlogGrid posts={data.posts.items} variant="editorial" />

		{#if data.posts.totalPages > 1}
			<nav class="päger" aria-label="Sidindelning">
				{#if data.posts.page > 1}
					<a href={`/blog?page=${data.posts.page - 1}${data.archive ? '&archive=true' : ''}`}>Föregående</a>
				{/if}
				<span>Sida {data.posts.page} av {data.posts.totalPages}</span>
				{#if data.posts.page < data.posts.totalPages}
					<a href={`/blog?page=${data.posts.page + 1}${data.archive ? '&archive=true' : ''}`}>Nästa</a>
				{/if}
			</nav>
		{/if}
	</div>
</section>

<style>
	header {
		max-width: 740px;
		margin: 0 auto 2.5rem;
		text-align: center;
	}

	.blog-index__logo {
		width: min(100%, 34rem);
		max-height: 12.5rem;
		margin: 0 auto 1rem;
		object-fit: contain;
	}

	.filters,
	.päger {
		display: flex;
		flex-wrap: wrap;
		justify-content: center;
		gap: 0.75rem;
		margin-top: 1.25rem;
	}

	.päger {
		align-items: center;
		color: var(--color-muted);
	}

	.päger a {
		border: 1px solid var(--color-border);
		border-radius: 999px;
		background: var(--color-surface);
		padding: 0.55rem 0.9rem;
		font-weight: 800;
	}
</style>
