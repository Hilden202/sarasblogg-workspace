<script lang="ts">
	import { tick } from 'svelte';

	export let value = '';
	export let id = 'content';
	export let label = 'Innehåll';

	let textarea: HTMLTextAreaElement;

	async function insert(prefix: string, suffix = '', placeholder = 'text') {
		const start = textarea.selectionStart ?? value.length;
		const end = textarea.selectionEnd ?? value.length;
		const selected = value.slice(start, end) || placeholder;
		value = `${value.slice(0, start)}${prefix}${selected}${suffix}${value.slice(end)}`;
		await tick();
		textarea.focus();
		textarea.setSelectionRange(start + prefix.length, start + prefix.length + selected.length);
	}

	function insertLink() {
		insert('<a href="https://">', '</a>', 'länktext');
	}

	function insertList() {
		insert('<ul>\n\t<li>', '</li>\n</ul>', 'punkt');
	}
</script>

<div class="rich-editor">
	<label for={id}>{label}</label>
	<div class="toolbar" aria-label="Textformatering">
		<button type="button" on:click={() => insert('<h2>', '</h2>', 'Mellanrubrik')}>H2</button>
		<button type="button" aria-label="Fet stil" on:click={() => insert('<strong>', '</strong>', 'fet text')}>B</button>
		<button type="button" aria-label="Kursiv stil" on:click={() => insert('<em>', '</em>', 'kursiv text')}>I</button>
		<button type="button" on:click={insertLink}>Länk</button>
		<button type="button" on:click={insertList}>Lista</button>
		<button type="button" on:click={() => insert('<blockquote>', '</blockquote>', 'citat')}>Citat</button>
	</div>
	<textarea bind:this={textarea} {id} bind:value rows="12"></textarea>
	<div class="preview">
		<p>Förhandsvisning</p>
		<div class="prose">{@html value || '<p></p>'}</div>
	</div>
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

	.toolbar {
		display: flex;
		flex-wrap: wrap;
		gap: 0.4rem;
	}

	.toolbar button {
		min-width: 2.35rem;
		min-height: 2.2rem;
		border: 1px solid var(--color-border);
		border-radius: 999px;
		background: rgba(255, 250, 244, 0.78);
		color: var(--color-heading);
		font-size: 0.84rem;
		font-weight: 900;
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

	.preview {
		padding: 1rem;
		border: 1px dashed rgba(217, 155, 121, 0.45);
		border-radius: var(--radius-soft);
		background: rgba(255, 250, 244, 0.58);
	}

	.preview > p {
		margin: 0 0 0.6rem;
		color: var(--color-muted);
		font-size: 0.78rem;
		font-weight: 800;
		letter-spacing: 0.06em;
		text-transform: uppercase;
	}
</style>
