<script lang="ts">
	import Button from '$lib/components/ui/Button.svelte';
	import FormField from '$lib/components/forms/FormField.svelte';
	import RichTextEditor from '$lib/components/forms/RichTextEditor.svelte';
	import type { AdminBlogPostDto, BloggImageDto, BlogPostWriteRequest } from '$lib/types/blog';
	import { toLocalDateTimeInput } from '$lib/utils/dates';
	import { resolveMediaUrl } from '$lib/utils/routes';

	export let post: AdminBlogPostDto | null = null;
	export let isSaving = false;
	export let isUploading = false;
	export let canManageImages = false;
	export let submitLabel = 'Spara';
	export let onSave: (request: BlogPostWriteRequest, files: File[]) => void | Promise<void> = () => {};
	export let onCancel: () => void = () => {};
	export let onDeleteImage: (image: BloggImageDto) => void | Promise<void> = () => {};
	export let onMakeCover: (image: BloggImageDto) => void | Promise<void> = () => {};
	export let uploadEditorImage: ((file: File) => Promise<string>) | undefined;

	let title = '';
	let author = '';
	let content = '';
	let launchDateLocal = '';
	let hidden = false;
	let isArchived = false;
	let imageInput: HTMLInputElement;
	let selectedFiles: File[] = [];
	let currentPostKey: number | 'new' = 'new';

	$: if (post) {
		title = post.title ?? '';
		author = post.author ?? '';
		content = post.content ?? '';
		launchDateLocal = toLocalDateTimeInput(post.launchDate);
		hidden = post.hidden;
		isArchived = post.isArchived;
	} else {
		title = '';
		author = '';
		content = '';
		launchDateLocal = '';
		hidden = false;
		isArchived = false;
	}
	$: if ((post?.id ?? 'new') !== currentPostKey) {
		currentPostKey = post?.id ?? 'new';
		selectedFiles = [];
		if (imageInput) imageInput.value = '';
	}

	function getRequest(): BlogPostWriteRequest {
		return {
			title: title.trim() || null,
			author: author.trim() || null,
			content,
			launchDateLocal: launchDateLocal || null,
			hidden,
			isArchived
		};
	}

	function handleSubmit() {
		onSave(getRequest(), selectedFiles);
	}

	function handleFiles(event: Event) {
		const input = event.currentTarget as HTMLInputElement;
		selectedFiles = Array.from(input.files ?? []);
	}

	function clearFiles() {
		selectedFiles = [];
		if (imageInput) imageInput.value = '';
	}
</script>

<form class="post-editor" on:submit|preventDefault={handleSubmit}>
	<div class="two-column">
		<FormField label="Titel" id="post-title">
			<input id="post-title" bind:value={title} placeholder="Rubrik" />
		</FormField>
		<FormField label="Författare" id="post-author">
			<input id="post-author" bind:value={author} placeholder="Sara Gustafsson" />
		</FormField>
	</div>

	<FormField label="Publiceringstid" id="post-launch">
		<input id="post-launch" type="datetime-local" bind:value={launchDateLocal} />
	</FormField>

	<RichTextEditor bind:value={content} id="post-content" label="Innehåll" height={550} uploadImage={uploadEditorImage} />

	{#if canManageImages}
		<section class="image-manager" aria-labelledby="post-images-title">
			<div>
				<h3 id="post-images-title">Bilder</h3>
				<p>JPG, PNG, WebP eller GIF. Maxstorlek kontrolleras av API:t.</p>
			</div>
			<FormField label="Lägg till bilder" id="post-images">
				<input
					bind:this={imageInput}
					id="post-images"
					type="file"
					multiple
					accept=".jpg,.jpeg,.png,.webp,.gif,image/jpeg,image/png,image/webp,image/gif"
					on:change={handleFiles}
				/>
			</FormField>
			{#if selectedFiles.length > 0}
				<div class="pending-files">
					<strong>Valda filer</strong>
					<ul>
						{#each selectedFiles as file}
							<li>{file.name}</li>
						{/each}
					</ul>
					<button type="button" on:click={clearFiles}>Rensa val</button>
				</div>
			{/if}
			{#if post?.images?.length}
				<div class="image-grid">
					{#each [...post.images].sort((a, b) => a.order - b.order || a.id - b.id) as image, index (image.id)}
						<article>
							<img src={resolveMediaUrl(image.filePath)} alt={`Bild ${index + 1} för inlägget`} />
							<div>
								<span>{index === 0 ? 'Första bild' : `Bild ${index + 1}`}</span>
								<button type="button" disabled={index === 0 || isUploading} on:click={() => onMakeCover(image)}>Gör först</button>
								<button type="button" class="danger" disabled={isUploading} on:click={() => onDeleteImage(image)}>Ta bort</button>
							</div>
						</article>
					{/each}
				</div>
			{:else if post}
				<p class="muted">Inga bilder är kopplade till inlägget ännu.</p>
			{:else}
				<p class="muted">Bilder laddas upp när inlägget sparas.</p>
			{/if}
		</section>
	{/if}

	<div class="checks">
		<label><input type="checkbox" bind:checked={hidden} /> Dolt</label>
		<label><input type="checkbox" bind:checked={isArchived} /> Arkiverat</label>
	</div>

	<div class="form-actions">
		<Button type="button" variant="ghost" disabled={isSaving || isUploading} on:click={onCancel}>Avbryt</Button>
		<Button type="submit" disabled={isSaving || isUploading}>{isSaving || isUploading ? 'Sparar...' : submitLabel}</Button>
	</div>
</form>

<style>
	.post-editor {
		display: grid;
		gap: 1rem;
	}

	.checks {
		display: flex;
		flex-wrap: wrap;
		gap: 1rem;
	}

	.image-manager {
		display: grid;
		gap: 1rem;
		padding: 1rem;
		border: 1px dashed rgba(217, 155, 121, 0.42);
		border-radius: var(--radius-soft);
		background: rgba(255, 250, 244, 0.55);
	}

	h3 {
		margin: 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: 1.75rem;
	}

	.image-manager p {
		margin: 0.15rem 0 0;
		color: var(--color-muted);
	}

	.pending-files {
		display: grid;
		gap: 0.4rem;
		color: var(--color-muted);
	}

	.pending-files ul {
		margin: 0;
		padding-left: 1.2rem;
	}

	.image-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(170px, 1fr));
		gap: 0.85rem;
	}

	.image-grid article {
		overflow: hidden;
		border: 1px solid var(--color-border);
		border-radius: var(--radius-soft);
		background: var(--color-surface);
	}

	.image-grid img {
		width: 100%;
		aspect-ratio: 4 / 3;
		object-fit: cover;
	}

	.image-grid article > div {
		display: grid;
		gap: 0.45rem;
		padding: 0.75rem;
	}

	.image-grid span {
		color: var(--color-heading);
		font-weight: 800;
	}

	.image-manager button {
		border: 1px solid var(--color-border);
		border-radius: 999px;
		background: var(--color-surface);
		color: var(--color-heading);
		padding: 0.4rem 0.65rem;
		font-weight: 800;
	}

	.image-manager button.danger {
		color: #9b3f35;
	}

	.image-manager button:disabled {
		cursor: not-allowed;
		opacity: 0.55;
	}

	.checks label {
		display: inline-flex;
		align-items: center;
		gap: 0.45rem;
		color: var(--color-heading);
		font-weight: 800;
	}

	.form-actions {
		display: flex;
		flex-wrap: wrap;
		justify-content: flex-end;
		gap: 0.65rem;
	}

	@media (max-width: 640px) {
		.form-actions {
			display: grid;
			grid-template-columns: 1fr;
		}
	}
</style>
