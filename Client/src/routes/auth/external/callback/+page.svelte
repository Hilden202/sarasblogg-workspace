<script lang="ts">
	import { page } from '$app/stores';
	import { onMount } from 'svelte';
	import AuthPanel from '$lib/components/auth/AuthPanel.svelte';
	import Button from '$lib/components/ui/Button.svelte';
	import { getFriendlyApiMessage } from '$lib/api/apiErrors';
	import { exchangeExternalLoginCode } from '$lib/services/authService';
	import { toasts } from '$lib/stores/toastStore';
	import { ensureBasePath, isSafeLocalPath, routes } from '$lib/utils/routes';

	let error = '';

	function safeReturnUrl(value: string | null) {
		if (!isSafeLocalPath(value)) return routes.home;
		return ensureBasePath(value);
	}

	onMount(async () => {
		const code = $page.url.searchParams.get('code');
		const returnUrl = safeReturnUrl($page.url.searchParams.get('returnUrl'));

		if (!code) {
			error = 'Google-inloggningen saknade en giltig kod.';
			return;
		}

		try {
			const fetchFn = window.fetch.bind(window);
			await exchangeExternalLoginCode(fetchFn, code);
			toasts.success('Du är inloggad.');
			window.location.replace(returnUrl);
		} catch (err) {
			error = getFriendlyApiMessage(err, 'Google-inloggningen kunde inte slutföras.');
			toasts.error(error);
		}
	});
</script>

<svelte:head>
	<title>Google-inloggning | SarasBlogg</title>
</svelte:head>

<AuthPanel
	title="Google-inloggning"
	text={error ? 'Vi kunde inte slutföra inloggningen automatiskt.' : 'Vi slutför inloggningen.'}
>
	{#if error}
		<p class="status-text status-text--error">{error}</p>
		<Button href={routes.login} variant="secondary">Till inloggning</Button>
	{:else}
		<p class="status-text">Ett ögonblick...</p>
	{/if}
</AuthPanel>
