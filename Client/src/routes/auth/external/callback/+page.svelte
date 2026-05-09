<script lang="ts">
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import { onMount } from 'svelte';
	import AuthPanel from '$lib/components/auth/AuthPanel.svelte';
	import Button from '$lib/components/ui/Button.svelte';
	import { getFriendlyApiMessage } from '$lib/api/apiErrors';
	import {
		exchangeExternalLoginCode,
		getCurrentUser,
		mapToFrontendUser
	} from '$lib/services/authService';
	import { auth } from '$lib/stores/authStore';
	import { toasts } from '$lib/stores/toastStore';
	import { routes } from '$lib/utils/routes';

	let error = '';

	function safeReturnUrl(value: string | null) {
		if (!value || !value.startsWith('/') || value.startsWith('//')) return routes.home;
		return value;
	}

	onMount(async () => {
		const code = $page.url.searchParams.get('code');
		const returnUrl = safeReturnUrl($page.url.searchParams.get('returnUrl'));

		if (!code) {
			error = 'Google-inloggningen saknade en giltig kod.';
			return;
		}

		try {
			await exchangeExternalLoginCode(window.fetch.bind(window), code);
			const me = await getCurrentUser(window.fetch.bind(window));
			if (me) auth.setUser(mapToFrontendUser(me));
			toasts.success('Du är inloggad.');
			await goto(returnUrl, { replaceState: true });
		} catch (err) {
			error = getFriendlyApiMessage(err, 'Google-inloggningen kunde inte slutföras.');
			toasts.error(error);
		}
	});
</script>

<svelte:head>
	<title>Google-inloggning | SarasBlogg</title>
</svelte:head>

<AuthPanel title="Google-inloggning" text={error ? 'Vi kunde inte slutföra inloggningen automatiskt.' : 'Vi slutför inloggningen.'}>
	{#if error}
		<p class="status-text status-text--error">{error}</p>
		<Button href={routes.login} variant="secondary">Till inloggning</Button>
	{:else}
		<p class="status-text">Ett ögonblick...</p>
	{/if}
</AuthPanel>
