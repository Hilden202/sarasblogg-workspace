<script lang="ts">
	import '$lib/styles/global.css';
	import Navbar from '$lib/components/layout/Navbar.svelte';
	import Footer from '$lib/components/layout/Footer.svelte';
	import CookieConsent from '$lib/components/layout/CookieConsent.svelte';
	import Toast from '$lib/components/ui/Toast.svelte';
	import ConfirmDialog from '$lib/components/ui/ConfirmDialog.svelte';
	import { page } from '$app/stores';
	import { brand } from '$lib/config/site';
	import { auth } from '$lib/stores/authStore';
	import { staticAsset } from '$lib/utils/routes';

	export let data;

	const topLeftFlower = staticAsset('/images/logo/top-left-corner-flower-ny.png');
	const bottomRightFlower = staticAsset('/images/logo/bottom-right-corner-flower-ny.png');
	const appShellStyle = `--app-flower-top-left: url("${topLeftFlower}"); --app-flower-bottom-right: url("${bottomRightFlower}");`;
	let cookieConsent: CookieConsent;

	function isExternalAuthCallbackPath(pathname: string) {
		return pathname.replace(/\/+$/, '').endsWith('/auth/external/callback');
	}

	$: isExternalAuthCallback =
		data.isExternalAuthCallback ?? isExternalAuthCallbackPath($page.url.pathname);
	$: if (!isExternalAuthCallback) {
		auth.setUser(data.user ?? null);
	}
</script>

<svelte:head>
	<link rel="icon" href={brand.favicon} />
	<meta name="theme-color" content="#fbf4ea" />
</svelte:head>

{#if isExternalAuthCallback}
	<slot />
{:else}
	<div class="app-shell" style={appShellStyle}>
		<Navbar />
		<main class="page-main">
			<slot />
		</main>
		<Footer on:privacy={() => cookieConsent?.openPrivacy()} />
		<CookieConsent bind:this={cookieConsent} />
		<Toast />
		<ConfirmDialog />
	</div>
{/if}
