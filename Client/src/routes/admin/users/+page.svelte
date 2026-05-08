<script lang="ts">
	import { invalidateAll } from '$app/navigation';
	import AdminTable from '$lib/components/admin/AdminTable.svelte';
	import { getFriendlyApiMessage } from '$lib/api/apiErrors';
	import { addUserRole, changeUserName, deleteUser, removeUserRole, sendResetLink } from '$lib/services/userService';
	import { confirmDialog } from '$lib/stores/confirmStore';
	import { toasts } from '$lib/stores/toastStore';
	import type { UserDto } from '$lib/types/user';

	export let data;

	let selectedRole: Record<string, string> = {};
	let userNames: Record<string, string> = {};
	let busyUserId: string | null = null;
	let manualResetLink = '';

	$: for (const user of data.users) {
		if (userNames[user.id] === undefined) userNames[user.id] = user.userName ?? '';
	}

	function isSystemAdmin(user: UserDto) {
		return (user.email ?? '').toLowerCase() === 'admin@sarasblogg.se';
	}

	async function addRole(user: UserDto) {
		if (!data.canManageUsers || isSystemAdmin(user)) return;
		const role = selectedRole[user.id];
		if (!role) return;
		busyUserId = user.id;
		try {
			await addUserRole(fetch, user.id, role);
			await invalidateAll();
			toasts.success('Rollen lades till.');
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Rollen kunde inte läggas till.'));
		} finally {
			busyUserId = null;
		}
	}

	async function removeRole(user: UserDto, role: string) {
		if (!data.canManageUsers || isSystemAdmin(user)) return;
		const confirmed = await confirmDialog.ask({
			title: 'Ta bort roll',
			message: `Ta bort rollen ${role} från ${user.email ?? user.userName}?`,
			confirmLabel: 'Ta bort',
			tone: 'danger'
		});
		if (!confirmed) return;

		busyUserId = user.id;
		try {
			await removeUserRole(fetch, user.id, role);
			await invalidateAll();
			toasts.success('Rollen togs bort.');
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Rollen kunde inte tas bort.'));
		} finally {
			busyUserId = null;
		}
	}

	async function removeUser(user: UserDto) {
		if (!data.canManageUsers || isSystemAdmin(user)) return;
		const confirmed = await confirmDialog.ask({
			title: 'Ta bort användare',
			message: `Vill du ta bort ${user.email ?? user.userName}?`,
			confirmLabel: 'Ta bort',
			tone: 'danger'
		});
		if (!confirmed) return;

		busyUserId = user.id;
		try {
			await deleteUser(fetch, user.id);
			await invalidateAll();
			toasts.success('Användaren togs bort.');
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Användaren kunde inte tas bort.'));
		} finally {
			busyUserId = null;
		}
	}

	async function updateUsername(user: UserDto) {
		const newUserName = userNames[user.id]?.trim();
		if (!data.canManageUsers || !newUserName || newUserName === user.userName || isSystemAdmin(user)) return;

		busyUserId = user.id;
		try {
			const result = await changeUserName(fetch, user.id, { newUserName });
			if (!result.succeeded) {
				toasts.error(result.message ?? 'Användarnamnet kunde inte ändras.');
				return;
			}
			await invalidateAll();
			toasts.success('Användarnamnet ändrades.');
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Användarnamnet kunde inte ändras.'));
		} finally {
			busyUserId = null;
		}
	}

	async function resetPassword(user: UserDto) {
		if (!data.canManageUsers || !user.email || isSystemAdmin(user)) return;
		const confirmed = await confirmDialog.ask({
			title: 'Återställ lösenord',
			message: `Skicka återställningslänk till ${user.email}?`,
			confirmLabel: 'Skicka'
		});
		if (!confirmed) return;

		busyUserId = user.id;
		manualResetLink = '';
		try {
			const result = await sendResetLink(fetch, user.email);
			manualResetLink = result.confirmEmailUrl ?? '';
			toasts.success(result.message ?? 'Återställningslänken har hanterats.');
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Återställningslänken kunde inte skickas.'));
		} finally {
			busyUserId = null;
		}
	}
</script>

<svelte:head>
	<title>Admin · Användare | SarasBlogg</title>
</svelte:head>

<section class="admin-page">
	<div>
		<p class="eyebrow">Admin</p>
		<h1>Användare</h1>
	</div>

	{#if data.error}
		<p class="status-text status-text--error">{data.error}</p>
	{/if}
	{#if !data.canManageUsers}
		<p class="status-text">Du kan läsa användarlistan. Rolländringar, namnbyten, återställning och borttagning kräver superadmin.</p>
	{/if}
	{#if manualResetLink}
		<p class="status-text status-text--success">
			Dev-länk: <a href={manualResetLink} target="_blank" rel="noreferrer">{manualResetLink}</a>
		</p>
	{/if}

	<AdminTable label="Användare">
		<thead>
			<tr>
				<th>Användare</th>
				<th>E-post</th>
				<th>Roller</th>
				<th>Användarnamn</th>
				<th>Ny roll</th>
				<th>Åtgärder</th>
			</tr>
		</thead>
		<tbody>
			{#each data.users as user (user.id)}
				<tr>
					<td>{user.userName}</td>
					<td>{user.email}</td>
					<td>
						<div class="role-list">
							{#each user.roles as role}
								<button
									type="button"
									disabled={!data.canManageUsers || isSystemAdmin(user) || busyUserId === user.id}
									on:click={() => removeRole(user, role)}
								>
									{role}{data.canManageUsers && !isSystemAdmin(user) ? ' ×' : ''}
								</button>
							{/each}
						</div>
					</td>
					<td>
						<input
							bind:value={userNames[user.id]}
							disabled={!data.canManageUsers || isSystemAdmin(user) || busyUserId === user.id}
							aria-label={`Användarnamn för ${user.email ?? user.userName}`}
						/>
					</td>
					<td>
						<select bind:value={selectedRole[user.id]} disabled={!data.canManageUsers || isSystemAdmin(user)}>
							<option value="">Välj roll</option>
							{#each data.roles as role}
								<option value={role}>{role}</option>
							{/each}
						</select>
					</td>
					<td>
						<div class="actions">
							<button type="button" disabled={!data.canManageUsers || isSystemAdmin(user) || busyUserId === user.id} on:click={() => updateUsername(user)}>Byt namn</button>
							<button type="button" disabled={!data.canManageUsers || isSystemAdmin(user) || busyUserId === user.id} on:click={() => addRole(user)}>Lägg till roll</button>
							<button type="button" disabled={!data.canManageUsers || isSystemAdmin(user) || busyUserId === user.id} on:click={() => resetPassword(user)}>Återställ</button>
							<button class="danger" type="button" disabled={!data.canManageUsers || isSystemAdmin(user) || busyUserId === user.id} on:click={() => removeUser(user)}>Ta bort</button>
						</div>
					</td>
				</tr>
			{/each}
		</tbody>
	</AdminTable>
</section>

<style>
	h1 {
		margin: 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(2.4rem, 5vw, 4rem);
	}

	select,
	input,
	button {
		border: 1px solid var(--color-border);
		border-radius: 999px;
		background: var(--color-surface);
		color: var(--color-heading);
		padding: 0.45rem 0.7rem;
		font-weight: 800;
	}

	input {
		width: min(100%, 220px);
	}

	button.danger {
		color: #9b3f35;
	}

	button:disabled,
	select:disabled,
	input:disabled {
		cursor: not-allowed;
		opacity: 0.58;
	}

	.status-text a {
		text-decoration: underline;
		text-underline-offset: 0.2em;
		overflow-wrap: anywhere;
	}

	.role-list {
		display: flex;
		flex-wrap: wrap;
		gap: 0.4rem;
	}
</style>
