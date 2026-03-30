<script lang="ts">
	import { login, getCurrentUser, mapToFrontendUser } from '$lib/services/authService';
	import { auth } from '$lib/stores/auth';
	import { goto } from '$app/navigation';

	let userNameOrEmail = '';
	let password = '';
	let error = '';

	async function handleLogin() {
		const success = await login(fetch, userNameOrEmail, password, true);

		if (!success) {
			error = 'Invalid credentials';
			return;
		}

		const me = await getCurrentUser(fetch);
		if (me) {
			auth.setUser(mapToFrontendUser(me));

			goto('/');
		}
	}
	function loginWithGoogle() {
		const returnUrl = window.location.origin;
		window.location.href = `https://localhost:5003/api/auth/external/google/start?returnUrl=${encodeURIComponent(returnUrl)}`;
	}
</script>

<h2>Login</h2>

<input bind:value={userNameOrEmail} placeholder="Email or username" />
<input type="password" bind:value={password} placeholder="Password" />

<button on:click={handleLogin}> Login </button>

<button on:click={loginWithGoogle}> Logga in med Google </button>

{#if error}
	<p style="color:red">{error}</p>
{/if}
