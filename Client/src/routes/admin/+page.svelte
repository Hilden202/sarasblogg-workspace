<script lang="ts">
	import Button from '$lib/components/ui/Button.svelte';
	import { routes } from '$lib/utils/routes';

	export let data;
</script>

<svelte:head>
	<title>Admin | SarasBlogg</title>
</svelte:head>

<section class="admin-page">
	<div class="admin-toolbar">
		<div>
			<p class="eyebrow">Admin</p>
			<h1>Översikt</h1>
		</div>
		<Button href={routes.adminPosts}>Hantera inlägg</Button>
	</div>

	<div class="stats">
		<a href={routes.adminPosts}><strong>{data.postCount ?? '–'}</strong><span>Inlägg</span></a>
		<a href={routes.adminComments}><strong>{data.commentCount ?? '–'}</strong><span>Kommentarer</span></a>
		<a href={routes.adminUsers}><strong>{data.userCount ?? '–'}</strong><span>Användare</span></a>
		<a href={routes.adminContactMessages}><strong>{data.contactMessageCount ?? '–'}</strong><span>Kontaktmeddelanden</span></a>
		<a href={routes.adminForbiddenWords}><strong>{data.forbiddenWordCount ?? '–'}</strong><span>Förbjudna ord</span></a>
	</div>
</section>

<style>
	h1 {
		margin: 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(2.4rem, 5vw, 4rem);
	}

	.stats {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
		gap: 1rem;
	}

	.stats a {
		padding: 1.25rem;
		border: 1px solid var(--color-border);
		border-radius: var(--radius-card);
		background: var(--color-surface);
		box-shadow: var(--shadow-small);
	}

	.stats strong {
		display: block;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: 3rem;
		line-height: 1;
	}

	.stats span {
		color: var(--color-muted);
		font-weight: 800;
	}

	@media (max-width: 720px) {
		.stats {
			grid-template-columns: 1fr;
		}
	}
</style>
