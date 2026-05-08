<script lang="ts">
	import { invalidateAll } from '$app/navigation';
	import Button from '$lib/components/ui/Button.svelte';
	import FormField from '$lib/components/forms/FormField.svelte';
	import FormSection from '$lib/components/forms/FormSection.svelte';
	import RichTextEditor from '$lib/components/forms/RichTextEditor.svelte';
	import { getFriendlyApiMessage } from '$lib/api/apiErrors';
	import { createAboutMe, deleteAboutImage, updateAboutMe, uploadAboutImage } from '$lib/services/aboutService';
	import { confirmDialog } from '$lib/stores/confirmStore';
	import { toasts } from '$lib/stores/toastStore';
	import { resolveMediaUrl } from '$lib/utils/routes';

	export let data;

	let title = data.about?.title ?? '';
	let content = data.about?.content ?? '';
	let image = data.about?.image ?? '';
	let imageFile: File | null = null;
	let removeImage = false;
	let isSaving = false;

	function handleImageChange(event: Event) {
		const input = event.currentTarget as HTMLInputElement;
		imageFile = input.files?.[0] ?? null;
		if (imageFile) removeImage = false;
	}

	async function save() {
		isSaving = true;
		try {
			let imageUrl = image.trim() || null;
			if (removeImage && image) {
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
			} else if (imageFile) {
				const uploaded = await uploadAboutImage(fetch, imageFile);
				imageUrl = uploaded.imageUrl ?? null;
			}

			if (data.about?.id) {
				await updateAboutMe(fetch, { id: data.about.id, title, content, image: imageUrl });
				toasts.success('Om mig-sidan är uppdaterad.');
			} else {
				await createAboutMe(fetch, { title, content, image: imageUrl });
				toasts.success('Om mig-sidan är skapad.');
			}
			image = imageUrl ?? '';
			imageFile = null;
			removeImage = false;
			await invalidateAll();
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Om mig-sidan kunde inte sparas.'));
		} finally {
			isSaving = false;
		}
	}
</script>

<svelte:head>
	<title>Admin · Om mig | SarasBlogg</title>
</svelte:head>

<section class="admin-page">
	<div>
		<p class="eyebrow">Innehåll</p>
		<h1>Om mig</h1>
	</div>

	<FormSection title="Redigera presentation" text="Innehåll och bild sparas via API:t. Beskärning från Razor är kvar som en separat parity-punkt.">
		<form class="form-grid" on:submit|preventDefault={save}>
			<FormField label="Titel" id="about-title">
				<input id="about-title" bind:value={title} />
			</FormField>
			<FormField label="Bild-URL" id="about-image">
				<input id="about-image" bind:value={image} />
			</FormField>
			<FormField label="Ladda upp bild" id="about-image-file">
				<input
					id="about-image-file"
					type="file"
					accept=".jpg,.jpeg,.png,.webp,image/jpeg,image/png,image/webp"
					on:change={handleImageChange}
				/>
			</FormField>
			{#if image}
				<div class="image-preview">
					<img src={resolveMediaUrl(image)} alt="Nuvarande Om mig-bild" />
					<label><input type="checkbox" bind:checked={removeImage} disabled={Boolean(imageFile)} /> Ta bort bild</label>
				</div>
			{/if}
			<RichTextEditor bind:value={content} id="about-content" label="Innehåll" />
			<Button type="submit" disabled={isSaving}>{isSaving ? 'Sparar...' : 'Spara'}</Button>
		</form>
	</FormSection>
</section>

<style>
	h1 {
		margin: 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(2.4rem, 5vw, 4rem);
	}

	.image-preview {
		display: grid;
		gap: 0.75rem;
		padding: 1rem;
		border: 1px solid var(--color-border);
		border-radius: var(--radius-soft);
		background: rgba(255, 250, 244, 0.62);
	}

	.image-preview img {
		width: min(100%, 320px);
		aspect-ratio: 4 / 3;
		border-radius: var(--radius-soft);
		object-fit: cover;
		box-shadow: var(--shadow-small);
	}

	.image-preview label {
		display: inline-flex;
		align-items: center;
		gap: 0.45rem;
		color: var(--color-heading);
		font-weight: 800;
	}
</style>
