<script lang="ts">
	import { invalidateAll } from '$app/navigation';
	import AdminTable from '$lib/components/admin/AdminTable.svelte';
	import { getFriendlyApiMessage } from '$lib/api/apiErrors';
	import { useClientFetch } from '$lib/api/clientFetch';
	import { deleteComment } from '$lib/services/commentService';
	import { confirmDialog } from '$lib/stores/confirmStore';
	import { toasts } from '$lib/stores/toastStore';
	import { formatDateTime } from '$lib/utils/dates';

	export let data;
	const getClientFetch = useClientFetch();

	let deletingId: number | null = null;

	async function remove(id: number) {
		const confirmed = await confirmDialog.ask({
			title: 'Ta bort kommentar',
			message: 'Vill du ta bort kommentaren?',
			confirmLabel: 'Ta bort',
			tone: 'danger'
		});
		if (!confirmed) return;

		deletingId = id;
		try {
			await deleteComment(getClientFetch(), id);
			await invalidateAll();
			toasts.success('Kommentaren togs bort.');
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Kommentaren kunde inte tas bort.'));
		} finally {
			deletingId = null;
		}
	}
</script>

<svelte:head>
	<title>Admin · Kommentarer | SarasBlogg</title>
</svelte:head>

<section class="admin-page">
	<div>
		<p class="eyebrow">Moderering</p>
		<h1>Kommentarer</h1>
	</div>

	{#if data.error}
		<p class="status-text status-text--error">{data.error}</p>
	{/if}

	<AdminTable label="Kommentarer">
		<thead>
			<tr>
				<th>Namn</th>
				<th>Kommentar</th>
				<th>Inlägg</th>
				<th>Skapad</th>
				<th>Åtgärder</th>
			</tr>
		</thead>
		<tbody>
			{#each data.comments as comment (comment.id)}
				<tr>
					<td>{comment.name}</td>
					<td>{comment.content}</td>
					<td>{comment.bloggId}</td>
					<td>{formatDateTime(comment.createdAt)}</td>
					<td><button type="button" disabled={deletingId === comment.id} on:click={() => remove(comment.id)}>{deletingId === comment.id ? 'Tar bort...' : 'Ta bort'}</button></td>
				</tr>
			{/each}
		</tbody>
	</AdminTable>
</section>

<style>
	h1 {
		margin: 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(2.4rem, 5vw, 4rem);
	}

	button {
		border: 1px solid var(--color-border);
		border-radius: 999px;
		background: var(--color-surface);
		color: #9b3f35;
		padding: 0.45rem 0.7rem;
		font-weight: 800;
	}

	button:disabled {
		cursor: not-allowed;
		opacity: 0.58;
	}
</style>
