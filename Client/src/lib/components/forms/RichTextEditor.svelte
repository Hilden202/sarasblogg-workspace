<script lang="ts">
	import { browser } from '$app/environment';
	import { onDestroy, onMount, tick } from 'svelte';

	export let value = '';
	export let id = 'content';
	export let label = 'Innehåll';
	export let height = 520;
	export let uploadImage: ((file: File) => Promise<string>) | undefined;

	type TinyMceEditor = {
		getContent: () => string;
		setContent: (content: string) => void;
		on: (events: string, callback: () => void) => void;
		remove: () => void;
	};

	type TinyMceGlobal = {
		init: (options: Record<string, unknown>) => Promise<TinyMceEditor[]> | TinyMceEditor[];
		get: (editorId: string) => TinyMceEditor | null;
	};

	let textarea: HTMLTextAreaElement;
	let editor: TinyMceEditor | null = null;
	let internalValue = '';
	let loadError = '';

	$: if (editor && value !== internalValue) {
		internalValue = value ?? '';
		editor.setContent(internalValue);
	}

	onMount(async () => {
		if (!browser) return;

		try {
			await loadTinyMce();
			await initEditor();
		} catch (error) {
			loadError = 'Texteditorn kunde inte laddas. Försök igen eller uppdatera sidan.';
			console.error(error);
		}
	});

	onDestroy(() => {
		editor?.remove();
		editor = null;
	});

	function loadTinyMce() {
		if (getTinyMce()) return Promise.resolve();

		return new Promise<void>((resolve, reject) => {
			const existing = document.querySelector<HTMLScriptElement>('script[data-tinymce-loader="true"]');
			if (existing) {
				existing.addEventListener('load', () => resolve(), { once: true });
				existing.addEventListener('error', () => reject(new Error('TinyMCE script failed to load.')), { once: true });
				return;
			}

			const script = document.createElement('script');
			script.src = '/lib/tinymce/tinymce.min.js';
			script.async = true;
			script.dataset.tinymceLoader = 'true';
			script.addEventListener('load', () => resolve(), { once: true });
			script.addEventListener('error', () => reject(new Error('TinyMCE script failed to load.')), { once: true });
			document.head.appendChild(script);
		});
	}

	async function initEditor() {
		await tick();

		const tinymce = getTinyMce();
		if (!tinymce || !textarea) return;

		tinymce.get(id)?.remove();
		textarea.value = value ?? '';
		internalValue = textarea.value;

		const result = await tinymce.init({
			target: textarea,
			base_url: '/lib/tinymce',
			suffix: '.min',
			license_key: 'gpl',
			branding: false,
			promotion: false,
			height,
			menubar: true,
			toolbar_mode: 'wrap',
			mobile: false,
			skin: 'oxide',
			content_css: 'default',
			plugins:
				'advlist autolink charmap code fullscreen image insertdatetime link lists media preview searchreplace visualblocks wordcount',
			toolbar:
				'undo redo | blocks | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | blockquote | link image | fullscreen code removeformat',
			block_formats: 'Paragraph=p; Heading 1=h1; Heading 2=h2; Heading 3=h3; Heading 4=h4',
			forced_root_block: 'p',
			convert_urls: false,
			automatic_uploads: true,
			file_picker_types: 'image',
			images_file_types: 'jpg,jpeg,png,webp',
			images_upload_handler: uploadEditorImage,
			content_style: `
				body {
					color: #594238;
					font-family: Inter, Arial, sans-serif;
					font-size: 16px;
					line-height: 1.75;
					padding: 1rem;
				}
				h1, h2, h3, h4 {
					color: #7a4a35;
					font-family: "Cormorant Garamond", Georgia, serif;
					line-height: 1.15;
				}
				blockquote {
					margin: 1.25rem 0;
					padding: 0.75rem 1rem;
					border-left: 4px solid #d99b79;
					background: rgba(255, 250, 244, 0.76);
					color: #6d5247;
					font-style: italic;
				}
				img {
					max-width: 100%;
					height: auto;
					border-radius: 0.8rem;
				}
				a {
					color: #9b5b3f;
				}
			`,
			setup: (instance: TinyMceEditor) => {
				instance.on('init', () => {
					editor = instance;
					internalValue = value ?? '';
					instance.setContent(internalValue);
				});
				instance.on('change input undo redo keyup SetContent', syncFromEditor);
			}
		});

		const editors = await Promise.resolve(result);
		editor = editors[0] ?? editor;
	}

	function syncFromEditor() {
		if (!editor) return;

		const nextValue = editor.getContent();
		if (nextValue === internalValue) return;

		internalValue = nextValue;
		value = nextValue;
	}

	async function uploadEditorImage(blobInfo: {
		blob: () => Blob;
		filename?: () => string;
	}) {
		if (!uploadImage) {
			throw new Error('Bilduppladdning är inte konfigurerad för editorn.');
		}

		const blob = blobInfo.blob();
		const filename = blobInfo.filename?.() ?? 'editor-image.png';
		const file = blob instanceof File ? blob : new File([blob], filename, { type: blob.type || 'image/png' });

		return uploadImage(file);
	}

	function getTinyMce() {
		return (window as Window & { tinymce?: TinyMceGlobal }).tinymce;
	}
</script>

<div class="rich-editor">
	<label for={id}>{label}</label>
	<textarea bind:this={textarea} {id} bind:value></textarea>
	{#if loadError}
		<p class="status-text status-text--error">{loadError}</p>
	{/if}
</div>

<style>
	.rich-editor {
		display: grid;
		gap: 0.75rem;
	}

	label {
		color: var(--color-heading);
		font-weight: 800;
	}

	textarea {
		width: 100%;
		min-height: 18rem;
		padding: 1rem;
		border: 1px solid var(--color-border);
		border-radius: var(--radius-soft);
		background: rgba(255, 250, 244, 0.92);
		color: var(--color-text);
		resize: vertical;
	}

	:global(.tox-tinymce) {
		border-color: var(--color-border) !important;
		border-radius: var(--radius-soft) !important;
		box-shadow: var(--shadow-small);
	}

	:global(.tox .tox-toolbar__primary),
	:global(.tox .tox-toolbar-overlord),
	:global(.tox .tox-menubar),
	:global(.tox .tox-edit-area__iframe),
	:global(.tox .tox-statusbar) {
		background: rgba(255, 250, 244, 0.96) !important;
	}
</style>
