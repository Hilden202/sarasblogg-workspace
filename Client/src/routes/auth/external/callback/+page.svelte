<script lang="ts">
	import { goto, invalidate } from '$app/navigation';
	import { page } from '$app/stores';
	import { onMount } from 'svelte';
	import AuthPanel from '$lib/components/auth/AuthPanel.svelte';
	import Button from '$lib/components/ui/Button.svelte';
	import { getFriendlyApiMessage } from '$lib/api/apiErrors';
	import { exchangeExternalLoginCode, getCurrentUser } from '$lib/services/authService';
	import { toasts } from '$lib/stores/toastStore';
	import { ensureBasePath, isSafeLocalPath, routes } from '$lib/utils/routes';

	let error = '';
	const sessionRestoreDelays = [80, 160, 320, 640];

	function safeReturnUrl(value: string | null) {
		if (!isSafeLocalPath(value)) return routes.home;
		return ensureBasePath(value);
	}

	function delay(ms: number) {
		return new Promise((resolve) => setTimeout(resolve, ms));
	}

	async function waitForSessionRestore(fetchFn: typeof fetch) {
		for (const delayMs of sessionRestoreDelays) {
			await delay(delayMs);

			try {
				const user = await getCurrentUser(fetchFn);
				if (user) return true;
			} catch {
				// The final document navigation below still gives the browser a fresh session read.
			}
		}

		return false;
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
			const sessionRestored = await waitForSessionRestore(fetchFn);
			toasts.success('Du är inloggad.');
			if (sessionRestored) {
				await invalidate('auth:session');
				await goto(returnUrl, { replaceState: true, invalidateAll: true });
				return;
			}
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
