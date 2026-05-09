<script lang="ts">
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import AuthPanel from '$lib/components/auth/AuthPanel.svelte';
	import Button from '$lib/components/ui/Button.svelte';
	import FormField from '$lib/components/forms/FormField.svelte';
	import { useClientFetch } from '$lib/api/clientFetch';
	import { getFriendlyApiMessage } from '$lib/api/apiErrors';
	import { getCurrentUser, getGoogleLoginUrl, login, mapToFrontendUser } from '$lib/services/authService';
	import { auth } from '$lib/stores/authStore';
	import { toasts } from '$lib/stores/toastStore';
	import { routes } from '$lib/utils/routes';

	const getClientFetch = useClientFetch();

	let userNameOrEmail = '';
	let password = '';
	let rememberMe = true;
	let isSaving = false;
	let error = '';

	$: returnUrl = $page.url.searchParams.get('returnUrl') || routes.profile;

	async function handleLogin() {
		error = '';
		isSaving = true;
		try {
			const apiFetch = getClientFetch();
			await login(apiFetch, userNameOrEmail, password, rememberMe);
			const me = await getCurrentUser(apiFetch);
			if (me) auth.setUser(mapToFrontendUser(me));
			toasts.success('Du är inloggad.');
			await goto(returnUrl);
		} catch (err) {
			error = getFriendlyApiMessage(err, 'Inloggningen misslyckades.');
			toasts.error(error);
		} finally {
			isSaving = false;
		}
	}

	function loginWithGoogle() {
		window.location.href = getGoogleLoginUrl(window.location.origin, returnUrl);
	}
</script>

<svelte:head>
	<title>Logga in | SarasBlogg</title>
</svelte:head>

<AuthPanel title="Välkommen tillbaka" text="Logga in för att kommentera, hantera din profil och komma åt adminytor när du har behörighet.">
	<form class="form-grid" on:submit|preventDefault={handleLogin}>
		<FormField label="E-post eller användarnamn" id="login-identity">
			<input id="login-identity" bind:value={userNameOrEmail} autocomplete="username" required />
		</FormField>

		<FormField label="Lösenord" id="login-password">
			<input id="login-password" type="password" bind:value={password} autocomplete="current-password" required />
		</FormField>

		<label class="check"><input type="checkbox" bind:checked={rememberMe} /> Kom ihåg mig</label>

		<Button type="submit" disabled={isSaving} full>{isSaving ? 'Loggar in...' : 'Logga in'}</Button>

		<button class="google-button" type="button" on:click={loginWithGoogle}>
			<span aria-hidden="true">G</span>
			Fortsätt med Google
		</button>

		{#if error}
			<p class="status-text status-text--error">{error}</p>
		{/if}

		<p class="auth-link">Inget konto än? <a href={routes.register}>Skapa ett här</a>.</p>
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

	.google-button {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		gap: 0.65rem;
		min-height: 2.85rem;
		border: 1px solid var(--color-border);
		border-radius: 999px;
		background: #fff;
		color: var(--color-heading);
		font-weight: 900;
	}

	.google-button span {
		display: grid;
		place-items: center;
		width: 1.55rem;
		height: 1.55rem;
		border-radius: 999px;
		background: rgba(244, 217, 202, 0.72);
		color: #8b4d38;
	}

	.auth-link {
		margin: 0;
		color: var(--color-muted);
		text-align: center;
	}

	.auth-link a {
		color: #9f664f;
		font-weight: 900;
	}
</style>
