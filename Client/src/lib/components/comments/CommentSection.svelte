<script lang="ts">
	import Button from '$lib/components/ui/Button.svelte';
	import FormField from '$lib/components/forms/FormField.svelte';
	import { getFriendlyApiMessage } from '$lib/api/apiErrors';
	import { createComment, deleteComment } from '$lib/services/commentService';
	import { auth } from '$lib/stores/authStore';
	import { confirmDialog } from '$lib/stores/confirmStore';
	import { toasts } from '$lib/stores/toastStore';
	import type { CommentDto } from '$lib/types/comment';
	import { formatDateTime } from '$lib/utils/dates';

	export let bloggId: number;
	export let comments: CommentDto[] = [];

	let localComments: CommentDto[] = [];
	let name = '';
	let content = '';
	let isSaving = false;
	let error = '';
	let visibleOlderCount = 0;
	let deletingId: number | null = null;

	$: localComments = comments;
	$: orderedComments = [...localComments].sort((a, b) => Date.parse(a.createdAt) - Date.parse(b.createdAt));
	$: splitIndex = Math.max(0, orderedComments.length - 10);
	$: olderComments = orderedComments.slice(0, splitIndex);
	$: recentComments = orderedComments.slice(splitIndex);
	$: visibleOlderComments = olderComments.slice(Math.max(0, olderComments.length - visibleOlderCount));
	$: remainingOlderCount = Math.max(0, olderComments.length - visibleOlderCount);

	async function submitComment() {
		error = '';
		if (!content.trim()) {
			error = 'Skriv en kommentar först.';
			return;
		}

		isSaving = true;
		try {
			const created = await createComment(fetch, {
				bloggId,
				name: $auth.user ? null : name.trim() || null,
				content: content.trim()
			});
			localComments = [...localComments, created];
			name = '';
			content = '';
			toasts.success('Kommentaren är publicerad.');
		} catch (err) {
			error = getFriendlyApiMessage(err, 'Kommentaren kunde inte publiceras.');
			toasts.error(error);
		} finally {
			isSaving = false;
		}
	}

	async function remove(id: number) {
		const confirmed = await confirmDialog.ask({
			title: 'Ta bort kommentar',
			message: 'Vill du ta bort kommentaren? Det går inte att ångra.',
			confirmLabel: 'Ta bort',
			tone: 'danger'
		});

		if (!confirmed) return;

		deletingId = id;
		try {
			await deleteComment(fetch, id);
			localComments = localComments.filter((comment) => comment.id !== id);
			toasts.success('Kommentaren togs bort.');
		} catch (err) {
			toasts.error(getFriendlyApiMessage(err, 'Kommentaren kunde inte tas bort.'));
		} finally {
			deletingId = null;
		}
	}

	function showOlder() {
		visibleOlderCount = Math.min(olderComments.length, visibleOlderCount + 10);
	}
</script>

<section class="comments" id="comments">
	<div class="narrow">
		<header>
			<p class="eyebrow">Samtal</p>
			<h2>Kommentarer</h2>
			<p class="muted">{localComments.length} {localComments.length === 1 ? 'kommentar' : 'kommentarer'}</p>
		</header>

		<form class="comment-form card" on:submit|preventDefault={submitComment}>
			{#if $auth.user}
				<p class="signed-in">Skriver som <strong>{$auth.user.displayName}</strong></p>
			{/if}
			{#if !$auth.user}
				<FormField label="Namn" id="comment-name">
					<input id="comment-name" bind:value={name} autocomplete="name" placeholder="Gäst" />
				</FormField>
			{/if}
			<FormField label="Kommentar" id="comment-content">
				<textarea id="comment-content" bind:value={content} rows="5" placeholder="Skriv några rader..." required></textarea>
			</FormField>
			<div class="form-actions">
				<Button type="submit" disabled={isSaving}>{isSaving ? 'Publicerar...' : 'Publicera'}</Button>
			</div>
			{#if error}
				<p class="status-text status-text--error">{error}</p>
			{/if}
		</form>

		<div class="comment-list">
			{#if localComments.length === 0}
				<p class="muted">Inga kommentarer ännu.</p>
			{:else}
				{#if remainingOlderCount > 0}
					<button class="load-older" type="button" on:click={showOlder}>
						Visa {Math.min(10, remainingOlderCount)} äldre ({remainingOlderCount} återstår)
					</button>
				{/if}
				{#each [...visibleOlderComments, ...recentComments] as comment (comment.id)}
					<article class="comment">
						<div>
							<strong>{comment.name}</strong>
							<span>{formatDateTime(comment.createdAt)}</span>
							{#if comment.topRole}
								<em>{comment.topRole}</em>
							{/if}
						</div>
						<p>{comment.content}</p>
						{#if comment.canDelete}
							<button type="button" disabled={deletingId === comment.id} on:click={() => remove(comment.id)}>
								{deletingId === comment.id ? 'Tar bort...' : 'Ta bort'}
							</button>
						{/if}
					</article>
				{/each}
			{/if}
		</div>
	</div>
</section>

<style>
	.comments {
		padding: 2rem 0 0;
	}

	header {
		margin-bottom: 1rem;
		text-align: center;
	}

	h2 {
		margin: 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(2rem, 4vw, 3rem);
	}

	header p:not(.eyebrow) {
		margin: 0.25rem 0 0;
	}

	.comment-form {
		display: grid;
		gap: 1rem;
		padding: 1.25rem;
	}

	.signed-in {
		margin: 0;
		color: var(--color-muted);
	}

	.signed-in strong {
		color: var(--color-heading);
	}

	.form-actions {
		display: flex;
		justify-content: flex-end;
	}

	.comment-list {
		display: grid;
		gap: 1rem;
		margin-top: 1.25rem;
	}

	.load-older {
		justify-self: center;
		border: 1px solid var(--color-border);
		border-radius: 999px;
		background: var(--color-surface);
		color: var(--color-heading);
		padding: 0.45rem 0.8rem;
		font-weight: 800;
	}

	.comment {
		padding: 1rem;
		border: 1px solid var(--color-border);
		border-radius: var(--radius-soft);
		background: rgba(255, 250, 244, 0.72);
	}

	.comment div {
		display: flex;
		flex-wrap: wrap;
		align-items: center;
		gap: 0.55rem;
	}

	.comment strong {
		color: var(--color-heading);
	}

	.comment span {
		color: var(--color-muted);
		font-size: 0.88rem;
	}

	.comment em {
		border-radius: 999px;
		background: rgba(143, 162, 132, 0.16);
		color: #586b4f;
		padding: 0.1rem 0.5rem;
		font-size: 0.78rem;
		font-style: normal;
		font-weight: 800;
	}

	.comment p {
		margin: 0.6rem 0 0;
	}

	.comment button {
		margin-top: 0.65rem;
		border: 0;
		background: transparent;
		color: #9b3f35;
		font-weight: 800;
	}

	.comment button:disabled {
		cursor: not-allowed;
		opacity: 0.58;
	}
</style>
