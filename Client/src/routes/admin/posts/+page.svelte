<script lang="ts">
	import { invalidateAll } from '$app/navigation';
	import AdminPostCard from '$lib/components/admin/AdminPostCard.svelte';
	import PostEditor from '$lib/components/admin/PostEditor.svelte';
	import Button from '$lib/components/ui/Button.svelte';
	import Modal from '$lib/components/ui/Modal.svelte';
	import { getFriendlyApiMessage } from '$lib/api/apiErrors';
	import { createPost, deletePost, togglePostHidden, updatePost } from '$lib/services/blogService';
	import { deleteBlogImage, updateBlogImageOrder, uploadBlogImage } from '$lib/services/blogImageService';
	import { uploadEditorImage as uploadEditorImageFile } from '$lib/services/editorUploadService';
	import { confirmDialog } from '$lib/stores/confirmStore';
	import { toasts } from '$lib/stores/toastStore';
	import type { AdminBlogPostDto, BloggImageDto, BlogPostWriteRequest } from '$lib/types/blog';

	export let data;

	let selected: AdminBlogPostDto | null = null;
	let editorOpen = false;
	let isSaving = false;
	let isUploading = false;
	let busyPostId: number | null = null;

	$: posts = (data.posts ?? []) as AdminBlogPostDto[];
	$: publishedPosts = posts.filter((post) => !post.hidden);
	$: hiddenPosts = posts.filter((post) => post.hidden);

	function openCreate() {
		selected = null;
		editorOpen = true;
	}

	function openEdit(post: AdminBlogPostDto) {
		selected = post;
		editorOpen = true;
	}

	function closeEditor() {
		if (isSaving || isUploading) return;
		editorOpen = false;
		selected = null;
	}

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
			editorOpen = false;
			selected = null;
			await invalidateAll();
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Inlägget kunde inte sparas.'));
		} finally {
			isSaving = false;
			isUploading = false;
		}
	}

	async function uploadEmbeddedImage(file: File) {
		try {
			return await uploadEditorImageFile(fetch, file, selected?.id ?? 0);
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Editorbilden kunde inte laddas upp.'));
			throw error;
		}
	}

	async function toggleHidden(post: AdminBlogPostDto) {
		busyPostId = post.id;
		try {
			await togglePostHidden(fetch, post.id);
			await invalidateAll();
			toasts.success(post.hidden ? 'Inlägget visas igen.' : 'Inlägget är dolt.');
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Synligheten kunde inte ändras.'));
		} finally {
			busyPostId = null;
		}
	}

	async function remove(post: AdminBlogPostDto) {
		if (!post.hidden) {
			toasts.error('Dölj inlägget innan det tas bort.');
			return;
		}

		const confirmed = await confirmDialog.ask({
			title: 'Ta bort inlägg',
			message: `Vill du ta bort "${post.title || 'Utan titel'}" permanent?`,
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
			<Button variant="secondary" on:click={openCreate}>Nytt inlägg</Button>
		{/if}
	</div>

	{#if data.error}
		<p class="status-text status-text--error">{data.error}</p>
	{/if}

	{#if data.canManagePosts}
		<Modal open={editorOpen} title={selected ? 'Redigera inlägg' : 'Skapa inlägg'} size="wide" on:close={closeEditor}>
			{#key selected?.id ?? 'new'}
				<PostEditor
					post={selected}
					{isSaving}
					{isUploading}
					canManageImages={data.canManagePosts}
					submitLabel={selected ? 'Spara ändringar' : 'Publicera'}
					onSave={save}
					onCancel={closeEditor}
					onDeleteImage={removeImage}
					onMakeCover={makeCover}
					uploadEditorImage={uploadEmbeddedImage}
				/>
			{/key}
		</Modal>
	{:else}
		<p class="status-text">Du kan dölja och visa inlägg. Skapa, redigera, bilder och borttagning visas för superadmin.</p>
	{/if}

	<section class="post-section" aria-labelledby="published-posts-title">
		<header class="post-section__header">
			<div>
				<p class="eyebrow">Publicerat</p>
				<h2 id="published-posts-title">Publicerade inlägg</h2>
			</div>
			<span class="section-count">{publishedPosts.length}</span>
		</header>

		{#if publishedPosts.length}
			<div class="post-list">
				{#each publishedPosts as post (post.id)}
					<AdminPostCard
						{post}
						mode="published"
						busy={busyPostId === post.id}
						canManagePosts={data.canManagePosts}
						canToggleStatus={data.canToggleStatus}
						onEdit={openEdit}
						onToggleHidden={toggleHidden}
					/>
				{/each}
			</div>
		{:else}
			<p class="empty-state">Inga publicerade inlägg att visa.</p>
		{/if}
	</section>

	<div class="section-divider" role="separator" aria-hidden="true"></div>

	<section class="post-section post-section--hidden" aria-labelledby="hidden-posts-title">
		<header class="post-section__header">
			<div>
				<p class="eyebrow">Moderering</p>
				<h2 id="hidden-posts-title">Dolda inlägg</h2>
			</div>
			<span class="section-count">{hiddenPosts.length}</span>
		</header>

		{#if hiddenPosts.length}
			<div class="post-list">
				{#each hiddenPosts as post (post.id)}
					<AdminPostCard
						{post}
						mode="hidden"
						busy={busyPostId === post.id}
						canManagePosts={data.canManagePosts}
						canToggleStatus={data.canToggleStatus}
						onEdit={openEdit}
						onToggleHidden={toggleHidden}
						onDelete={remove}
					/>
				{/each}
			</div>
		{:else}
			<p class="empty-state">Inga dolda inlägg att visa.</p>
		{/if}
	</section>
</section>

<style>
	h1 {
		margin: 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(2.4rem, 5vw, 4rem);
	}

	h2 {
		margin: 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(1.8rem, 4vw, 2.65rem);
		line-height: 1;
	}

	.post-section {
		display: grid;
		gap: 0.9rem;
	}

	.post-section__header {
		display: flex;
		align-items: end;
		justify-content: space-between;
		gap: 1rem;
	}

	.section-count {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-width: 2.25rem;
		min-height: 2.25rem;
		padding: 0.35rem 0.65rem;
		border: 1px solid rgba(217, 155, 121, 0.34);
		border-radius: 999px;
		background: rgba(255, 250, 244, 0.72);
		color: var(--color-heading);
		font-weight: 900;
	}

	.post-list {
		display: grid;
		gap: 0.8rem;
	}

	.section-divider {
		height: 1px;
		margin: 0.5rem 0;
		background: linear-gradient(90deg, transparent, rgba(217, 155, 121, 0.56), transparent);
	}

	.post-section--hidden {
		padding-top: 0.25rem;
	}

	.empty-state {
		margin: 0;
		padding: 1rem;
		border: 1px dashed rgba(95, 74, 59, 0.18);
		border-radius: 0.75rem;
		background: rgba(255, 250, 244, 0.58);
		color: var(--color-muted);
		font-weight: 700;
	}

	@media (max-width: 640px) {
		.admin-toolbar {
			display: grid;
			grid-template-columns: 1fr;
		}
	}
</style>
