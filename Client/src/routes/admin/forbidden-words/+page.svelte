<script lang="ts">
	import { invalidateAll } from '$app/navigation';
	import Button from '$lib/components/ui/Button.svelte';
	import FormField from '$lib/components/forms/FormField.svelte';
	import FormSection from '$lib/components/forms/FormSection.svelte';
	import { useClientFetch } from '$lib/api/clientFetch';
	import { getFriendlyApiMessage } from '$lib/api/apiErrors';
	import { createForbiddenWord, deleteForbiddenWord } from '$lib/services/adminService';
	import { confirmDialog } from '$lib/stores/confirmStore';
	import { toasts } from '$lib/stores/toastStore';

	export let data;

	const getClientFetch = useClientFetch();

	let wordPattern = '';

	async function add() {
		if (!wordPattern.trim()) return;
		try {
			await createForbiddenWord(getClientFetch(), wordPattern.trim());
			wordPattern = '';
			await invalidateAll();
			toasts.success('Mönster lades till.');
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Monstret kunde inte läggas till.'));
		}
	}

	async function remove(id: number) {
		const confirmed = await confirmDialog.ask({
			title: 'Ta bort mönster',
			message: 'Vill du ta bort modereringsmönstret?',
			confirmLabel: 'Ta bort',
			tone: 'danger'
		});
		if (!confirmed) return;

		try {
			await deleteForbiddenWord(getClientFetch(), id);
			await invalidateAll();
			toasts.success('Mönster togs bort.');
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Monstret kunde inte tas bort.'));
		}
	}
</script>

<svelte:head>
	<title>Admin · Förbjudna ord | SarasBlogg</title>
</svelte:head>

<section class="admin-page">
	<div>
		<p class="eyebrow">Moderering</p>
		<h1>Förbjudna ord</h1>
	</div>

	{#if data.error}
		<p class="status-text status-text--error">{data.error}</p>
	{/if}

	<FormSection title="Lägg till mönster" text="API:t äger själva censur- och säkerhetsreglerna.">
		<form class="word-form" on:submit|preventDefault={add}>
			<FormField label="Regex eller ordmönster" id="word-pattern">
				<input id="word-pattern" bind:value={wordPattern} />
			</FormField>
			<Button type="submit">Lägg till</Button>
		</form>
	</FormSection>

	<div class="word-list">
		{#each data.words as word (word.id)}
			<div>
				<code>{word.wordPattern}</code>
				<button type="button" on:click={() => remove(word.id)}>Ta bort</button>
			</div>
		{/each}
	</div>
</section>

<style>
	h1 {
		margin: 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(2.4rem, 5vw, 4rem);
	}

	.word-form {
		display: grid;
		grid-template-columns: 1fr auto;
		gap: 1rem;
		align-items: end;
	}

	.word-list {
		display: grid;
		gap: 0.75rem;
	}

	.word-list div {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 1rem;
		padding: 1rem;
		border: 1px solid var(--color-border);
		border-radius: var(--radius-soft);
		background: var(--color-surface);
	}

	code {
		overflow-wrap: anywhere;
		color: var(--color-heading);
	}

	button {
		border: 1px solid var(--color-border);
		border-radius: 999px;
		background: transparent;
		color: #9b3f35;
		padding: 0.4rem 0.7rem;
		font-weight: 800;
	}

	@media (max-width: 620px) {
		.word-form,
		.word-list div {
			grid-template-columns: 1fr;
			flex-direction: column;
			align-items: stretch;
		}
	}
</style>
