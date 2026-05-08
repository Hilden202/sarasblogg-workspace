<script lang="ts">
	import AuthPanel from '$lib/components/auth/AuthPanel.svelte';
	import Button from '$lib/components/ui/Button.svelte';

	export let data;
</script>

<AuthPanel title="Google-inloggning" text="Vi kunde inte slutföra inloggningen automatiskt.">
	{#if data.error}
		<p class="status-text status-text--error">{data.error}</p>
	{/if}
	<Button href="/login" variant="secondary">Till inloggning</Button>
</AuthPanel>
