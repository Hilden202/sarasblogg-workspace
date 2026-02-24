<script lang="ts">
	export let data;
	const { user } = data;
</script>

{#if user}
	<h2>Logged in as {user.userName}</h2>
	<p>Email: {user.email}</p>
{:else}
	<h2>Not logged in</h2>
{/if}
