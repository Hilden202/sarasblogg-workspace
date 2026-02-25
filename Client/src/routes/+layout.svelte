<script lang="ts">
	import favicon from '$lib/assets/favicon.svg';
	import { auth } from '$lib/stores/auth';
	import { logout } from '$lib/services/authService';
	import { goto } from '$app/navigation';

	let { children } = $props();

	async function handleLogout() {
		await logout(fetch);
		auth.clear();
		goto('/');
	}
</script>

<svelte:head>
	<link rel="icon" href={favicon} />
</svelte:head>

<nav style="padding:1rem; border-bottom:1px solid #ccc;">
	{#if $auth.user}
		<span>
			Logged in as <strong>{$auth.user.displayName}</strong>
		</span>

		{#if auth.hasMinRole('admin')}
			<span style="margin-left:1rem;">(Admin)</span>
		{/if}

		<button style="margin-left:1rem;" on:click={handleLogout}> Logout </button>
	{:else}
		<a href="/login">Login</a>
	{/if}
</nav>

{@render children()}
