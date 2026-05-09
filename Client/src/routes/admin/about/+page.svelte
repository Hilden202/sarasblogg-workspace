<script lang="ts">
	import { invalidateAll } from '$app/navigation';
	import AboutEditor from '$lib/components/admin/AboutEditor.svelte';
	import Button from '$lib/components/ui/Button.svelte';
	import Modal from '$lib/components/ui/Modal.svelte';
	import { getFriendlyApiMessage } from '$lib/api/apiErrors';
	import { createAboutMe, deleteAboutImage, updateAboutMe, uploadAboutImage } from '$lib/services/aboutService';
	import { uploadEditorImage as uploadEditorImageFile } from '$lib/services/editorUploadService';
	import { confirmDialog } from '$lib/stores/confirmStore';
	import { toasts } from '$lib/stores/toastStore';
	import { resolveMediaUrl } from '$lib/utils/routes';

	export let data;

	type AboutSavePayload = {
		title: string;
		content: string;
		image: string;
		imageFile: File | null;
		removeImage: boolean;
	};

	let editorOpen = false;
	let isSaving = false;

	function openEditor() {
		editorOpen = true;
	}

	function closeEditor() {
		if (isSaving) return;
		editorOpen = false;
	}

	async function save(payload: AboutSavePayload) {
		isSaving = true;
		try {
			let imageUrl = payload.image.trim() || null;
			if (payload.removeImage && payload.image) {
				const confirmed = await confirmDialog.ask({
					title: 'Ta bort bild',
					message: 'Vill du ta bort den nuvarande Om mig-bilden?',
					confirmLabel: 'Ta bort',
					tone: 'danger'
				});
				if (!confirmed) {
					isSaving = false;
					return;
				}
				await deleteAboutImage(fetch);
				imageUrl = null;
			} else if (payload.imageFile) {
				const uploaded = await uploadAboutImage(fetch, payload.imageFile);
				imageUrl = uploaded.imageUrl ?? null;
			}

			if (data.about?.id) {
				await updateAboutMe(fetch, { id: data.about.id, title: payload.title, content: payload.content, image: imageUrl });
				toasts.success('Om mig-sidan är uppdaterad.');
			} else {
				await createAboutMe(fetch, { title: payload.title, content: payload.content, image: imageUrl });
				toasts.success('Om mig-sidan är skapad.');
			}
			editorOpen = false;
			await invalidateAll();
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Om mig-sidan kunde inte sparas.'));
		} finally {
			isSaving = false;
		}
	}

	async function uploadEmbeddedImage(file: File) {
		try {
			return await uploadEditorImageFile(fetch, file);
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Editorbilden kunde inte laddas upp.'));
			throw error;
		}
	}
</script>

<svelte:head>
	<title>Admin · Om mig | SarasBlogg</title>
</svelte:head>

<section class="admin-page">
	<div class="admin-toolbar">
		<div>
			<p class="eyebrow">Innehåll</p>
			<h1>Om mig</h1>
		</div>
		<Button variant="secondary" on:click={openEditor}>{data.about ? 'Redigera' : 'Skapa'}</Button>
	</div>

	<article class="about-admin-preview">
		{#if data.about?.image}
			<img src={resolveMediaUrl(data.about.image)} alt="" />
		{/if}
		<div>
			<p class="eyebrow">Aktuell sida</p>
			<h2>{data.about?.title || 'Om mig'}</h2>
			<div class="prose about-admin-preview__content">
				{@html data.about?.content || '<p>Ingen presentation är publicerad ännu.</p>'}
			</div>
		</div>
	</article>

	<Modal open={editorOpen} title={data.about ? 'Redigera Om mig' : 'Skapa Om mig'} size="wide" on:close={closeEditor}>
		{#key data.about?.id ?? 'new'}
			<AboutEditor
				about={data.about}
				{isSaving}
				onSave={save}
				onCancel={closeEditor}
				uploadEditorImage={uploadEmbeddedImage}
			/>
		{/key}
	</Modal>
</section>

<style>
	h1 {
		margin: 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(2.4rem, 5vw, 4rem);
	}

	.about-admin-preview {
		display: grid;
		grid-template-columns: minmax(180px, 260px) minmax(0, 1fr);
		gap: clamp(1rem, 3vw, 1.75rem);
		align-items: start;
		padding: clamp(1rem, 3vw, 1.35rem);
		border: 1px solid var(--color-border);
		border-radius: 0.75rem;
		background: rgba(255, 250, 244, 0.72);
		box-shadow: var(--shadow-small);
	}

	.about-admin-preview > img {
		width: 100%;
		aspect-ratio: 0.82;
		border-radius: 0.75rem;
		object-fit: cover;
		box-shadow: var(--shadow-small);
	}

	h2 {
		margin: 0.25rem 0 1rem;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(2rem, 4vw, 3rem);
		line-height: 1;
	}

	.about-admin-preview__content {
		max-width: 70ch;
	}

	@media (max-width: 720px) {
		.about-admin-preview {
			grid-template-columns: 1fr;
		}

		.about-admin-preview > img {
			width: min(100%, 280px);
		}
	}
</style>
