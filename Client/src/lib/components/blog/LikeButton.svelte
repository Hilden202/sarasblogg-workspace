<script lang="ts">
	import { page } from '$app/stores';
	import { useClientFetch } from '$lib/api/clientFetch';
	import { getFriendlyApiMessage } from '$lib/api/apiErrors';
	import { likePost, unlikePost } from '$lib/services/likeService';
	import { auth } from '$lib/stores/authStore';
	import { toasts } from '$lib/stores/toastStore';
	import type { LikeDto } from '$lib/types/like';
	import { routes } from '$lib/utils/routes';

	export let bloggId: number;
	export let initialLike: LikeDto | null = null;

	const getClientFetch = useClientFetch();

	let count = initialLike?.count ?? 0;
	let liked = initialLike?.liked ?? false;
	let isSaving = false;

	$: loginUrl = `${routes.login}?returnUrl=${encodeURIComponent($page.url.pathname + $page.url.search)}`;

	async function toggle() {
		if (!$auth.user) return;
		isSaving = true;
		try {
			const apiFetch = getClientFetch();
			const result = liked ? await unlikePost(apiFetch, bloggId) : await likePost(apiFetch, bloggId);
			count = result.count;
			liked = !liked;
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Gillamarkeringen kunde inte sparas.'));
		} finally {
			isSaving = false;
		}
	}
</script>

<div class="likes" aria-live="polite">
	<button
		type="button"
		class:liked
		disabled={!$auth.user || isSaving}
		aria-pressed={liked}
		aria-label={liked ? 'Ångra gilla' : 'Gilla inlägget'}
		on:click={toggle}
	>
		<span aria-hidden="true">{liked ? '♥' : '♡'}</span>
		<strong>{count}</strong>
	</button>
	{#if !$auth.user}
		<a href={loginUrl}>Logga in för att gilla</a>
	{/if}
</div>

<style>
	.likes {
		display: flex;
		align-items: center;
		justify-content: center;
		gap: 0.8rem;
		margin: 0 auto 1.5rem;
		color: var(--color-muted);
	}

	button {
		display: inline-flex;
		align-items: center;
		gap: 0.42rem;
		min-height: 2.7rem;
		padding: 0.5rem 0.9rem;
		border: 1px solid rgba(217, 155, 121, 0.5);
		border-radius: 999px;
		background: var(--color-surface);
		color: var(--color-heading);
		font-weight: 900;
		box-shadow: var(--shadow-small);
	}

	button span {
		color: var(--color-rose);
		font-size: 1.25rem;
		line-height: 1;
	}

	button.liked {
		background: rgba(244, 217, 202, 0.82);
	}

	button:disabled {
		cursor: not-allowed;
		opacity: 0.62;
	}

	a {
		color: var(--color-muted);
		text-decoration: underline;
		text-underline-offset: 0.25em;
	}
</style>
