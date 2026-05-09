<script lang="ts">
	import { base } from '$app/paths';
	import '$lib/styles/global.css';
	import Navbar from '$lib/components/layout/Navbar.svelte';
	import Footer from '$lib/components/layout/Footer.svelte';
	import Toast from '$lib/components/ui/Toast.svelte';
	import ConfirmDialog from '$lib/components/ui/ConfirmDialog.svelte';
	import { brand } from '$lib/config/site';
	import { auth } from '$lib/stores/authStore';

	export let data;

	const appShellStyle = `--app-flower-top-left: url("${base}/images/logo/top-left-corner-flower-ny.png"); --app-flower-bottom-right: url("${base}/images/logo/bottom-right-corner-flower-ny.png");`;

	$: auth.setUser(data.user ?? null);
</script>

<svelte:head>
	<link rel="icon" href={brand.favicon} />
	<meta name="theme-color" content="#fbf4ea" />
</svelte:head>

<div class="app-shell" style={appShellStyle}>
	<Navbar />
	<main class="page-main">
		<slot />
	</main>
	<Footer />
	<Toast />
	<ConfirmDialog />
</div>
