<script lang="ts">
	import { page } from '$app/stores';
	import AuthPanel from '$lib/components/auth/AuthPanel.svelte';
	import { getGoogleLoginUrl } from '$lib/services/authService';
	import { ensureBasePath, isSafeLocalPath, routes } from '$lib/utils/routes';
	import { base } from '$app/paths';

	function safeReturnUrl(value: string | null) {
		if (!isSafeLocalPath(value)) return routes.home;
		return ensureBasePath(value);
	}

	$: returnUrl = safeReturnUrl($page.url.searchParams.get('returnUrl'));

	function loginWithGoogle() {
		window.location.href = getGoogleLoginUrl(
			window.location.origin,
			returnUrl,
			`${base}/auth/external/callback/`
		);
	}
</script>

<svelte:head>
	<title>Logga in | SarasBlogg</title>
</svelte:head>

<AuthPanel
	title="Välkommen tillbaka"
	text="Fortsätt med Google för att kommentera, hantera din profil och komma åt adminytor när du har behörighet."
>
	<button class="google-button" type="button" on:click={loginWithGoogle}>
		<span aria-hidden="true">G</span>
		Fortsätt med Google
	</button>
</AuthPanel>

<style>
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
</style>
