<script lang="ts">
	import AuthPanel from '$lib/components/auth/AuthPanel.svelte';
	import Button from '$lib/components/ui/Button.svelte';
	import FormField from '$lib/components/forms/FormField.svelte';
	import { useClientFetch } from '$lib/api/clientFetch';
	import { getFriendlyApiMessage } from '$lib/api/apiErrors';
	import { register } from '$lib/services/authService';
	import { toasts } from '$lib/stores/toastStore';
	import { routes } from '$lib/utils/routes';

	const getClientFetch = useClientFetch();

	let userName = '';
	let email = '';
	let password = '';
	let name = '';
	let birthYear: number | null = null;
	let subscribeNewPosts = true;
	let isSaving = false;
	let message = '';
	let confirmEmailUrl: string | null = null;
	let error = '';

	async function submit() {
		error = '';
		message = '';
		confirmEmailUrl = null;
		isSaving = true;
		try {
			const result = await register(getClientFetch(), {
				userName,
				email,
				password,
				name: name || null,
				birthYear,
				subscribeNewPosts
			});
			message = result.message || 'Kontot är skapat. Kontrollera din e-post för bekräftelse.';
			confirmEmailUrl = result.confirmEmailUrl ?? null;
			toasts.success(message);
		} catch (err) {
			error = getFriendlyApiMessage(err, 'Kontot kunde inte skapas.');
			toasts.error(error);
		} finally {
			isSaving = false;
		}
	}
</script>

<svelte:head>
	<title>Skapa konto | SarasBlogg</title>
</svelte:head>

<AuthPanel title="Skapa konto" text="Ett konto låter dig kommentera med din profil och följa framtida funktioner.">
	<form class="form-grid" on:submit|preventDefault={submit}>
		<div class="two-column">
			<FormField label="Användarnamn" id="register-username">
				<input id="register-username" bind:value={userName} autocomplete="username" required />
			</FormField>
			<FormField label="Namn" id="register-name">
				<input id="register-name" bind:value={name} autocomplete="name" />
			</FormField>
		</div>

		<FormField label="E-post" id="register-email">
			<input id="register-email" type="email" bind:value={email} autocomplete="email" required />
		</FormField>

		<div class="two-column">
			<FormField label="Lösenord" id="register-password">
				<input id="register-password" type="password" bind:value={password} autocomplete="new-password" required />
			</FormField>
			<FormField label="Födelseår" id="register-birthyear">
				<input id="register-birthyear" type="number" min="1900" max="2100" bind:value={birthYear} />
			</FormField>
		</div>

		<label class="check"><input type="checkbox" bind:checked={subscribeNewPosts} /> Få mejl vid nya inlägg</label>

		<Button type="submit" disabled={isSaving} full>{isSaving ? 'Skapar...' : 'Skapa konto'}</Button>

		{#if message}
			<p class="status-text status-text--success">{message}</p>
		{/if}
		{#if confirmEmailUrl}
			<a class="confirm-link" href={confirmEmailUrl}>Öppna bekräftelselänk</a>
		{/if}
		{#if error}
			<p class="status-text status-text--error">{error}</p>
		{/if}

		<p class="auth-link">Har du redan konto? <a href={routes.login}>Logga in</a>.</p>
	</form>
</AuthPanel>

<style>
	.check {
		display: inline-flex;
		align-items: center;
		gap: 0.5rem;
		color: var(--color-muted);
		font-weight: 700;
	}

	.confirm-link,
	.auth-link a {
		color: #9f664f;
		font-weight: 900;
	}

	.auth-link {
		margin: 0;
		color: var(--color-muted);
		text-align: center;
	}
</style>
