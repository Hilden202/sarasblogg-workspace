<script lang="ts">
	import { goto } from '$app/navigation';
	import AuthPanel from '$lib/components/auth/AuthPanel.svelte';
	import FormField from '$lib/components/forms/FormField.svelte';
	import Button from '$lib/components/ui/Button.svelte';
	import { useClientFetch } from '$lib/api/clientFetch';
	import { getFriendlyApiMessage } from '$lib/api/apiErrors';
	import { refreshSession } from '$lib/services/authService';
	import { changeMyUsername } from '$lib/services/userService';
	import { toasts } from '$lib/stores/toastStore';

	export let data;

	const getClientFetch = useClientFetch();

	let newUserName = '';
	let error = '';
	let isSaving = false;

	async function saveUsername() {
		const trimmedUserName = newUserName.trim();
		if (!trimmedUserName) {
			error = 'Skriv ett användarnamn först.';
			return;
		}

		error = '';
		isSaving = true;
		try {
			const fetchFn = getClientFetch();
			const result = await changeMyUsername(fetchFn, { newUserName: trimmedUserName });
			await refreshSession(fetchFn);
			toasts.success(result.message || 'Användarnamnet är sparat.');
			await goto(data.returnUrl);
		} catch (err) {
			error = getFriendlyApiMessage(err, 'Användarnamnet kunde inte sparas.');
			toasts.error(error);
		} finally {
			isSaving = false;
		}
	}
</script>

<svelte:head>
	<title>Välj användarnamn | SarasBlogg</title>
</svelte:head>

<AuthPanel
	title="Välj användarnamn"
	text="Du behöver välja ett användarnamn innan du kan fortsätta."
>
	<form class="username-form" on:submit|preventDefault={saveUsername}>
		<FormField label="Användarnamn" id="setup-username">
			<input id="setup-username" bind:value={newUserName} minlength="3" autocomplete="username" />
		</FormField>
		<Button type="submit" disabled={isSaving}>
			{isSaving ? 'Sparar...' : 'Spara användarnamn'}
		</Button>
		{#if error}
			<p class="status-text status-text--error">{error}</p>
		{/if}
	</form>
</AuthPanel>

<style>
	.username-form {
		display: grid;
		gap: 1rem;
	}
</style>
