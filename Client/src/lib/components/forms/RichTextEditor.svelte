<script lang="ts">
	import { browser } from '$app/environment';
	import { base } from '$app/paths';
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
		execCommand: (command: string, ui?: boolean, value?: unknown, ...args: unknown[]) => void;
		ui: {
			registry: {
				addMenuButton: (
					name: string,
					config: {
						text?: string;
						icon?: string;
						fetch: (callback: (items: unknown[]) => void) => void;
					}
				) => void;
			};
		};
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
	const toolbar =
		'undo redo | blocks fontfamily fontsize | forecolor backcolor highlight | bold italic underline strikethrough | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | blockquote table | link image media | preview fullscreen code removeformat';
	const mobileToolbar =
		'undo redo | blocks | bold italic underline | forecolor backcolor highlight | bullist numlist | alignleft aligncenter | link image | removeformat';
	const highlightColors = [
		{ text: 'Gul', color: '#fceea7' },
		{ text: 'Rosa', color: '#ffe5ec' },
		{ text: 'Grön', color: '#cde8d5' },
		{ text: 'Blå', color: '#e0f0ff' },
		{ text: 'Koppar', color: '#a87363' }
	];

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
			const existing = document.querySelector<HTMLScriptElement>(
				'script[data-tinymce-loader="true"]'
			);
			if (existing) {
				existing.addEventListener('load', () => resolve(), { once: true });
				existing.addEventListener(
					'error',
					() => reject(new Error('TinyMCE script failed to load.')),
					{ once: true }
				);
				return;
			}

			const script = document.createElement('script');
			script.src = `${base}/lib/tinymce/tinymce.min.js`;
			script.async = true;
			script.dataset.tinymceLoader = 'true';
			script.addEventListener('load', () => resolve(), { once: true });
			script.addEventListener('error', () => reject(new Error('TinyMCE script failed to load.')), {
				once: true
			});
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
			base_url: `${base}/lib/tinymce`,
			suffix: '.min',
			license_key: 'gpl',
			branding: false,
			promotion: false,
			height,
			menubar: true,
			toolbar_mode: 'wrap',
			mobile: {
				menubar: false,
				toolbar_mode: 'sliding',
				toolbar: mobileToolbar
			},
			skin: 'oxide',
			content_css: 'default',
			plugins:
				'advlist autolink charmap code fullscreen image insertdatetime link lists media preview searchreplace table visualblocks wordcount',
			toolbar,
			block_formats: 'Paragraph=p; Heading 1=h1; Heading 2=h2; Heading 3=h3',
			font_family_formats:
				'Cormorant Garamond=Cormorant Garamond,serif;' +
				'Roboto=Roboto,Arial,sans-serif;' +
				'Lato=Lato,Arial,sans-serif;' +
				'Merriweather=Merriweather,Georgia,serif;' +
				'Montserrat=Montserrat,Arial,sans-serif;' +
				'Arial=arial,helvetica,sans-serif;' +
				'Georgia=georgia,serif;' +
				"Times New Roman='Times New Roman',times,serif;" +
				'Verdana=verdana,geneva,sans-serif;' +
				"Courier New='Courier New',courier,monospace;" +
				"Lucida Handwriting='Lucida Handwriting',cursive;",
			font_size_formats: '8pt 9pt 10pt 11pt 12pt 14pt 16pt 18pt 24pt 36pt 48pt 72pt 96pt 120pt',
			color_map: [
				'7e6655',
				'Brun (Hjärtat)',
				'a87363',
				'Koppar',
				'c48a7d',
				'Rosa',
				'b87333',
				'Bronze',
				'd4af37',
				'Guld',
				'fceea7',
				'Highlight - Ljusgul',
				'ffe5ec',
				'Highlight - Rosa',
				'cde8d5',
				'Highlight - Ljusgrön',
				'e0f0ff',
				'Highlight - Ljusblå',
				'ffffff',
				'Vit',
				'000000',
				'Svart',
				'708090',
				'Slate Gray'
			],
			color_picker_callback: (callback: (value: string) => void, selectedColor?: string) => {
				const input = document.createElement('input');
				input.type = 'color';
				input.value = selectedColor || '#fceea7';
				input.addEventListener('change', () => callback(input.value), { once: true });
				input.click();
			},
			forced_root_block: 'p',
			convert_urls: false,
			automatic_uploads: true,
			file_picker_types: 'image',
			images_file_types: 'jpg,jpeg,png,webp',
			images_upload_handler: uploadEditorImage,
			formats: {
				alignleft: { block: 'p', styles: { 'text-align': 'left' } },
				aligncenter: { block: 'p', styles: { 'text-align': 'center' } },
				alignright: { block: 'p', styles: { 'text-align': 'right' } },
				alignjustify: { block: 'p', styles: { 'text-align': 'justify' } }
			},
			valid_styles: {
				'*': 'text-align,font-family,font-size,color,background-color,font-weight,font-style,text-decoration'
			},
			content_style: `
				body {
					color: #7e6655;
					font-family: "Cormorant Garamond", Georgia, serif;
					font-size: 16px;
					line-height: 1.6;
					padding: 1rem;
				}
				h1, h2, h3 {
					color: #7e6655;
					font-family: "Cormorant Garamond", Georgia, serif;
					letter-spacing: 0.05em;
					line-height: 1.1;
					text-transform: uppercase;
				}
				blockquote {
					margin: 1.5rem 0;
					padding: .75rem 1.25rem;
					border-left: 4px solid #c48a7d;
					background: #fdf3eb;
				}
				img {
					max-width: 100%;
					height: auto;
					border-radius: 0.8rem;
				}
				a {
					color: #a87363;
					text-decoration: underline;
				}
				ul, ol {
					padding-left: 1.5rem;
				}
			`,
			setup: (instance: TinyMceEditor) => {
				instance.ui.registry.addMenuButton('highlight', {
					text: 'Highlight',
					icon: 'highlight-bg-color',
					fetch: (callback) => {
						callback([
							...highlightColors.map((item) => ({
								type: 'menuitem',
								text: item.text,
								onAction: () =>
									instance.execCommand('mceApplyTextcolor', false, item.color, 'hilitecolor')
							})),
							{
								type: 'menuitem',
								text: 'Ta bort',
								onAction: () => instance.execCommand('RemoveTextcolor', false, 'hilitecolor')
							}
						]);
					}
				});
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

	async function uploadEditorImage(blobInfo: { blob: () => Blob; filename?: () => string }) {
		if (!uploadImage) {
			throw new Error('Bilduppladdning är inte konfigurerad för editorn.');
		}

		const blob = blobInfo.blob();
		const filename = blobInfo.filename?.() ?? 'editor-image.png';
		const file =
			blob instanceof File ? blob : new File([blob], filename, { type: blob.type || 'image/png' });

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
		max-width: 100%;
		border-color: var(--color-border) !important;
		border-radius: var(--radius-soft) !important;
		box-shadow: var(--shadow-small);
	}

	:global(.tox-tinymce-aux) {
		z-index: 120 !important;
	}

	:global(.tox .tox-toolbar__primary),
	:global(.tox .tox-toolbar-overlord),
	:global(.tox .tox-menubar),
	:global(.tox .tox-edit-area__iframe),
	:global(.tox .tox-statusbar) {
		background: rgba(255, 250, 244, 0.96) !important;
	}

	:global(.tox .tox-toolbar__primary) {
		flex-wrap: wrap !important;
	}

	:global(.tox .tox-toolbar__group) {
		max-width: 100%;
	}

	@media (max-width: 640px) {
		.rich-editor {
			margin-inline: -0.25rem;
		}

		:global(.tox .tox-toolbar__group) {
			padding-inline: 0.15rem !important;
		}

		:global(.tox .tox-tbtn) {
			width: 2.1rem !important;
			height: 2.1rem !important;
		}
	}
</style>
