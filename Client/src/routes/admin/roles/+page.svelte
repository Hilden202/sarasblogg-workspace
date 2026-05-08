<script lang="ts">
	import { invalidateAll } from '$app/navigation';
	import Button from '$lib/components/ui/Button.svelte';
	import FormField from '$lib/components/forms/FormField.svelte';
	import FormSection from '$lib/components/forms/FormSection.svelte';
	import { getFriendlyApiMessage } from '$lib/api/apiErrors';
	import { createRole, deleteRole } from '$lib/services/adminService';
	import { confirmDialog } from '$lib/stores/confirmStore';
	import { toasts } from '$lib/stores/toastStore';

	export let data;

	let roleName = '';
	const protectedRoles = new Set(['superadmin', 'admin', 'superuser', 'user']);

	async function add() {
		if (!roleName.trim()) return;
		try {
			await createRole(fetch, roleName.trim());
			roleName = '';
			await invalidateAll();
			toasts.success('Rollen skapades.');
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Rollen kunde inte skapas.'));
		}
	}

	async function remove(role: string) {
		if (protectedRoles.has(role.toLowerCase())) return;

		const confirmed = await confirmDialog.ask({
			title: 'Ta bort roll',
			message: `Vill du ta bort rollen ${role}?`,
			confirmLabel: 'Ta bort',
			tone: 'danger'
		});
		if (!confirmed) return;

		try {
			await deleteRole(fetch, role);
			await invalidateAll();
			toasts.success('Rollen togs bort.');
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Rollen kunde inte tas bort.'));
		}
	}
</script>

<svelte:head>
	<title>Admin · Roller | SarasBlogg</title>
</svelte:head>

<section class="admin-page">
	<div>
		<p class="eyebrow">Superadmin</p>
		<h1>Roller</h1>
	</div>

	{#if data.error}
		<p class="status-text status-text--error">{data.error}</p>
	{/if}

	<FormSection title="Skapa roll">
		<form class="role-form" on:submit|preventDefault={add}>
			<FormField label="Rollnamn" id="role-name">
				<input id="role-name" bind:value={roleName} />
			</FormField>
			<Button type="submit">Skapa</Button>
		</form>
	</FormSection>

	<div class="role-grid">
		{#each data.roles as role}
			<div class="role-card">
				<strong>{role}</strong>
				{#if protectedRoles.has(role.toLowerCase())}
					<span>Grundroll</span>
				{:else}
					<button type="button" on:click={() => remove(role)}>Ta bort</button>
				{/if}
			</div>
		{/each}
	</div>
</section>

<style>
	h1 {
		margin: 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(2.4rem, 5vw, 4rem);
	}

	.role-form {
		display: grid;
		grid-template-columns: 1fr auto;
		gap: 1rem;
		align-items: end;
	}

	.role-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
		gap: 1rem;
	}

	.role-card {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 1rem;
		padding: 1rem;
		border: 1px solid var(--color-border);
		border-radius: var(--radius-soft);
		background: var(--color-surface);
	}

	button {
		border: 1px solid var(--color-border);
		border-radius: 999px;
		background: transparent;
		color: #9b3f35;
		padding: 0.4rem 0.7rem;
		font-weight: 800;
	}

	.role-card span {
		color: var(--color-muted);
		font-size: 0.86rem;
		font-weight: 800;
	}

	@media (max-width: 620px) {
		.role-form {
			grid-template-columns: 1fr;
		}
	}
</style>
