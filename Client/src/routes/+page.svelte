<script lang="ts">
	import { auth } from '$lib/stores/auth';
</script>

{#if $auth.user}
	<h2>Logged in as {$auth.user.displayName}</h2>
	<p>Email: {$auth.user.email}</p>

	{#if auth.hasMinRole('admin')}
		<p>Admin content</p>
	{/if}
{:else}
	<h2>Not logged in</h2>
{/if}
