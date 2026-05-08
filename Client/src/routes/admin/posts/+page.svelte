<script lang="ts">
	import { invalidateAll } from '$app/navigation';
	import AdminTable from '$lib/components/admin/AdminTable.svelte';
	import PostEditor from '$lib/components/admin/PostEditor.svelte';
	import Button from '$lib/components/ui/Button.svelte';
	import FormSection from '$lib/components/forms/FormSection.svelte';
	import { getFriendlyApiMessage } from '$lib/api/apiErrors';
	import { createPost, deletePost, togglePostArchived, togglePostHidden, updatePost } from '$lib/services/blogService';
	import { deleteBlogImage, updateBlogImageOrder, uploadBlogImage } from '$lib/services/blogImageService';
	import { confirmDialog } from '$lib/stores/confirmStore';
	import { toasts } from '$lib/stores/toastStore';
	import type { AdminBlogPostDto, BloggImageDto, BlogPostWriteRequest } from '$lib/types/blog';
	import { formatDate } from '$lib/utils/dates';
	import { resolveMediaUrl } from '$lib/utils/routes';

	export let data;

	let selected: AdminBlogPostDto | null = null;
	let isSaving = false;
	let isUploading = false;
	let busyPostId: number | null = null;

	async function save(request: BlogPostWriteRequest, files: File[]) {
		isSaving = true;
		try {
			let savedId = selected?.id;
			if (selected) {
				await updatePost(fetch, selected.id, request);
				toasts.success('Inlägget är uppdaterat.');
			} else {
				const created = await createPost(fetch, request);
				savedId = created.id;
				toasts.success('Inlägget är skapat.');
			}
			if (savedId && files.length > 0) {
				isUploading = true;
				for (const file of files) {
					await uploadBlogImage(fetch, savedId, file);
				}
				toasts.success(files.length === 1 ? 'Bilden laddades upp.' : 'Bilderna laddades upp.');
			}
			selected = null;
			await invalidateAll();
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Inlägget kunde inte sparas.'));
		} finally {
			isSaving = false;
			isUploading = false;
		}
	}

	async function toggleHidden(post: AdminBlogPostDto) {
		busyPostId = post.id;
		try {
			await togglePostHidden(fetch, post.id);
			await invalidateAll();
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Synligheten kunde inte ändras.'));
		} finally {
			busyPostId = null;
		}
	}

	async function toggleArchived(post: AdminBlogPostDto) {
		busyPostId = post.id;
		try {
			await togglePostArchived(fetch, post.id);
			await invalidateAll();
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Arkivstatus kunde inte ändras.'));
		} finally {
			busyPostId = null;
		}
	}

	async function remove(post: AdminBlogPostDto) {
		const confirmed = await confirmDialog.ask({
			title: 'Ta bort inlägg',
			message: `Vill du ta bort "${post.title || 'Utan titel'}"?`,
			confirmLabel: 'Ta bort',
			tone: 'danger'
		});

		if (!confirmed) return;

		busyPostId = post.id;
		try {
			await deletePost(fetch, post.id);
			await invalidateAll();
			toasts.success('Inlägget togs bort.');
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Inlägget kunde inte tas bort.'));
		} finally {
			busyPostId = null;
		}
	}

	async function removeImage(image: BloggImageDto) {
		const confirmed = await confirmDialog.ask({
			title: 'Ta bort bild',
			message: 'Vill du ta bort bilden från inlägget?',
			confirmLabel: 'Ta bort',
			tone: 'danger'
		});

		if (!confirmed || !selected) return;

		isUploading = true;
		try {
			await deleteBlogImage(fetch, image.id);
			selected = {
				...selected,
				images: (selected.images ?? []).filter((item) => item.id !== image.id)
			};
			await invalidateAll();
			toasts.success('Bilden togs bort.');
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Bilden kunde inte tas bort.'));
		} finally {
			isUploading = false;
		}
	}

	async function makeCover(image: BloggImageDto) {
		if (!selected?.images?.length) return;

		isUploading = true;
		try {
			const ordered = [
				image,
				...selected.images
					.filter((item) => item.id !== image.id)
					.sort((a, b) => a.order - b.order || a.id - b.id)
			].map((item, index) => ({ ...item, order: index }));
			await updateBlogImageOrder(fetch, selected.id, ordered);
			selected = { ...selected, images: ordered };
			await invalidateAll();
			toasts.success('Första bilden är uppdaterad.');
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Bildordningen kunde inte sparas.'));
		} finally {
			isUploading = false;
		}
	}
</script>

<svelte:head>
	<title>Admin · Inlägg | SarasBlogg</title>
</svelte:head>

<section class="admin-page">
	<div class="admin-toolbar">
		<div>
			<p class="eyebrow">Admin</p>
			<h1>Inlägg</h1>
		</div>
		{#if data.canManagePosts}
			<Button variant="secondary" on:click={() => (selected = null)}>Nytt inlägg</Button>
		{/if}
	</div>

	{#if data.error}
		<p class="status-text status-text--error">{data.error}</p>
	{/if}

	{#if data.canManagePosts}
		<FormSection title={selected ? 'Redigera inlägg' : 'Skapa inlägg'}>
			{#key selected?.id ?? 'new'}
				<PostEditor
					post={selected}
					{isSaving}
					{isUploading}
					canManageImages={data.canManagePosts}
					submitLabel={selected ? 'Spara ändringar' : 'Publicera'}
					onSave={save}
					onDeleteImage={removeImage}
					onMakeCover={makeCover}
				/>
			{/key}
		</FormSection>
	{:else}
		<p class="status-text">Du kan dölja eller arkivera inlägg. Skapa, redigera, bilder och borttagning visas för superadmin.</p>
	{/if}

	<AdminTable label="Inlägg">
		<thead>
			<tr>
				<th>Bild</th>
				<th>Titel</th>
				<th>Publicerad</th>
				<th>Status</th>
				<th>Visningar</th>
				<th>Åtgärder</th>
			</tr>
		</thead>
		<tbody>
			{#each data.posts as post (post.id)}
				<tr>
					<td>
						{#if post.images?.[0]}
							<img class="thumb" src={resolveMediaUrl(post.images[0].filePath)} alt="" />
						{:else}
							<span class="muted">Ingen</span>
						{/if}
					</td>
					<td>{post.title || 'Utan titel'}</td>
					<td>{formatDate(post.launchDate)}</td>
					<td>
						<span class="badge">{post.hidden ? 'Dold' : 'Synlig'}</span>
						{#if post.isArchived}<span class="badge badge--sage">Arkiv</span>{/if}
					</td>
					<td>{post.viewCount}</td>
					<td>
						<div class="actions">
							{#if data.canManagePosts}
								<button type="button" on:click={() => (selected = post)}>Redigera</button>
							{/if}
							{#if data.canToggleStatus}
								<button type="button" disabled={busyPostId === post.id} on:click={() => toggleHidden(post)}>{post.hidden ? 'Visa' : 'Dölj'}</button>
								<button type="button" disabled={busyPostId === post.id} on:click={() => toggleArchived(post)}>{post.isArchived ? 'Aktivera' : 'Arkivera'}</button>
							{/if}
							{#if data.canManagePosts}
								<button type="button" class="danger" disabled={busyPostId === post.id} on:click={() => remove(post)}>Ta bort</button>
							{/if}
						</div>
					</td>
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

	td button {
		border: 1px solid var(--color-border);
		border-radius: 999px;
		background: var(--color-surface);
		color: var(--color-heading);
		padding: 0.45rem 0.7rem;
		font-weight: 800;
	}

	td button.danger {
		color: #9b3f35;
	}

	td button:disabled {
		cursor: not-allowed;
		opacity: 0.58;
	}

	.thumb {
		width: 4.75rem;
		aspect-ratio: 4 / 3;
		border-radius: 0.65rem;
		object-fit: cover;
		box-shadow: var(--shadow-small);
	}
</style>
