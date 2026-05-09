<script lang="ts">
	import { onDestroy } from 'svelte';
	import Button from '$lib/components/ui/Button.svelte';
	import FormField from '$lib/components/forms/FormField.svelte';
	import RichTextEditor from '$lib/components/forms/RichTextEditor.svelte';
	import type { AboutMeDto } from '$lib/types/about';
	import { resolveMediaUrl } from '$lib/utils/routes';

	type SavePayload = {
		title: string;
		content: string;
		image: string;
		imageFile: File | null;
		removeImage: boolean;
	};

	export let about: AboutMeDto | null = null;
	export let isSaving = false;
	export let onSave: (payload: SavePayload) => void | Promise<void> = () => {};
	export let onCancel: () => void = () => {};
	export let uploadEditorImage: ((file: File) => Promise<string>) | undefined;

	let title = '';
	let content = '';
	let image = '';
	let imageFile: File | null = null;
	let removeImage = false;
	let imageInput: HTMLInputElement;
	let previewObjectUrl = '';
	let currentAboutKey: number | 'new' = 'new';

	$: if ((about?.id ?? 'new') !== currentAboutKey) {
		currentAboutKey = about?.id ?? 'new';
		title = about?.title ?? '';
		content = about?.content ?? '';
		image = about?.image ?? '';
		imageFile = null;
		removeImage = false;
		clearPreviewObjectUrl();
		if (imageInput) imageInput.value = '';
	}

	$: currentImagePreview = !removeImage && image.trim() ? resolveMediaUrl(image.trim()) : '';
	$: previewSrc = previewObjectUrl || currentImagePreview;
	$: hasImageSelection = Boolean(previewObjectUrl || image.trim());

	onDestroy(() => {
		clearPreviewObjectUrl();
	});

	function handleImageChange(event: Event) {
		const input = event.currentTarget as HTMLInputElement;
		imageFile = input.files?.[0] ?? null;
		removeImage = false;
		clearPreviewObjectUrl();

		if (imageFile) {
			previewObjectUrl = URL.createObjectURL(imageFile);
		}
	}

	function clearImageFile() {
		imageFile = null;
		clearPreviewObjectUrl();
		if (imageInput) imageInput.value = '';
	}

	function clearPreviewObjectUrl() {
		if (!previewObjectUrl) return;
		URL.revokeObjectURL(previewObjectUrl);
		previewObjectUrl = '';
	}

	function handleSubmit() {
		onSave({
			title,
			content,
			image,
			imageFile,
			removeImage
		});
	}
</script>

<form class="about-editor" on:submit|preventDefault={handleSubmit}>
	<div class="about-editor__fields">
		<FormField label="Titel" id="about-title">
			<input id="about-title" bind:value={title} />
		</FormField>

		<div class="image-panel">
			<div class="two-column">
				<FormField label="Bild-URL" id="about-image">
					<input id="about-image" bind:value={image} />
				</FormField>
				<FormField label="Ladda upp bild" id="about-image-file">
					<input
						bind:this={imageInput}
						id="about-image-file"
						type="file"
						accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp"
						on:change={handleImageChange}
					/>
				</FormField>
			</div>

			{#if hasImageSelection}
				<div class="image-preview">
					{#if previewSrc}
						<img src={previewSrc} alt="Förhandsvisning av Om mig-bild" />
					{:else}
						<div class="image-preview__empty">Bild markeras för borttagning</div>
					{/if}
					<div class="image-preview__actions">
						{#if imageFile}
							<button type="button" on:click={clearImageFile}>Rensa val</button>
						{/if}
						{#if image && !imageFile}
							<label><input type="checkbox" bind:checked={removeImage} /> Ta bort bild</label>
						{/if}
					</div>
				</div>
			{/if}
		</div>

		<RichTextEditor bind:value={content} id="about-content" label="Innehåll" height={440} uploadImage={uploadEditorImage} />
	</div>

	<aside class="about-editor__preview" aria-label="Förhandsvisning">
		{#if previewSrc}
			<img class="about-editor__preview-image" src={previewSrc} alt="" />
		{/if}
		<div>
			<p class="eyebrow">Förhandsvisning</p>
			<h3>{title || 'Om mig'}</h3>
			<div class="prose preview-prose">
				{@html content || '<p>Ingen presentation ännu.</p>'}
			</div>
		</div>
	</aside>

	<div class="form-actions">
		<Button type="button" variant="ghost" disabled={isSaving} on:click={onCancel}>Avbryt</Button>
		<Button type="submit" disabled={isSaving}>{isSaving ? 'Sparar...' : 'Spara'}</Button>
	</div>
</form>

<style>
	.about-editor {
		display: grid;
		grid-template-columns: minmax(0, 1.35fr) minmax(260px, 0.65fr);
		gap: 1.25rem;
	}

	.about-editor__fields {
		display: grid;
		gap: 1rem;
		min-width: 0;
	}

	.image-panel {
		display: grid;
		gap: 0.85rem;
		padding: 1rem;
		border: 1px dashed rgba(217, 155, 121, 0.42);
		border-radius: var(--radius-soft);
		background: rgba(255, 250, 244, 0.55);
	}

	.image-preview {
		display: grid;
		grid-template-columns: minmax(120px, 180px) minmax(0, 1fr);
		gap: 0.9rem;
		align-items: center;
	}

	.image-preview img {
		width: 100%;
		aspect-ratio: 4 / 3;
		border-radius: 0.75rem;
		object-fit: cover;
		box-shadow: var(--shadow-small);
	}

	.image-preview__empty {
		display: grid;
		place-items: center;
		min-height: 8rem;
		border: 1px solid var(--color-border);
		border-radius: 0.75rem;
		background: rgba(244, 217, 202, 0.32);
		color: var(--color-muted);
		font-weight: 800;
		text-align: center;
	}

	.image-preview__actions {
		display: flex;
		flex-wrap: wrap;
		gap: 0.65rem;
		align-items: center;
	}

	.image-preview label {
		display: inline-flex;
		align-items: center;
		gap: 0.45rem;
		color: var(--color-heading);
		font-weight: 800;
	}

	.image-preview button {
		border: 1px solid var(--color-border);
		border-radius: 999px;
		background: var(--color-surface);
		color: var(--color-heading);
		padding: 0.45rem 0.7rem;
		font-weight: 800;
	}

	.about-editor__preview {
		display: grid;
		align-self: start;
		gap: 0.9rem;
		min-width: 0;
		padding: 1rem;
		border: 1px solid var(--color-border);
		border-radius: var(--radius-soft);
		background: rgba(255, 250, 244, 0.72);
	}

	.about-editor__preview-image {
		width: min(100%, 220px);
		aspect-ratio: 0.82;
		border-radius: var(--radius-soft);
		object-fit: cover;
		box-shadow: var(--shadow-small);
	}

	h3 {
		margin: 0.25rem 0 0.75rem;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(1.8rem, 4vw, 2.4rem);
		line-height: 1;
	}

	.preview-prose {
		max-height: 24rem;
		overflow: auto;
		font-size: 0.95rem;
	}

	.form-actions {
		display: flex;
		flex-wrap: wrap;
		grid-column: 1 / -1;
		justify-content: flex-end;
		gap: 0.65rem;
	}

	@media (max-width: 900px) {
		.about-editor {
			grid-template-columns: 1fr;
		}

		.about-editor__preview {
			order: -1;
		}
	}

	@media (max-width: 640px) {
		.image-preview {
			grid-template-columns: 1fr;
		}

		.form-actions {
			display: grid;
			grid-template-columns: 1fr;
		}
	}
</style>
