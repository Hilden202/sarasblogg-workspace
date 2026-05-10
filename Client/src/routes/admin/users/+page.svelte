<script lang="ts">
	import { invalidateAll } from '$app/navigation';
	import AdminTable from '$lib/components/admin/AdminTable.svelte';
	import { useClientFetch } from '$lib/api/clientFetch';
	import { getFriendlyApiMessage } from '$lib/api/apiErrors';
	import { addUserRole, changeUserName, deleteUser, removeUserRole, sendResetLink } from '$lib/services/userService';
	import { confirmDialog } from '$lib/stores/confirmStore';
	import { toasts } from '$lib/stores/toastStore';
	import type { UserDto } from '$lib/types/user';

	export let data;

	const getClientFetch = useClientFetch();

	let userNames: Record<string, string> = {};
	let busyUserId: string | null = null;
	let manualResetLink = '';

	$: for (const user of data.users) {
		if (userNames[user.id] === undefined) userNames[user.id] = user.userName ?? '';
	}

	function isSystemAdmin(user: UserDto) {
		return (user.email ?? '').toLowerCase() === 'admin@sarasblogg.se';
	}

	function hasRole(user: UserDto, role: string) {
		return user.roles.some((userRole) => userRole.toLowerCase() === role.toLowerCase());
	}

	async function addRole(user: UserDto, role: string) {
		if (!data.canManageUsers || isSystemAdmin(user)) return;
		if (!role) return;
		busyUserId = user.id;
		try {
			await addUserRole(getClientFetch(), user.id, role);
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
			await removeUserRole(getClientFetch(), user.id, role);
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
			await deleteUser(getClientFetch(), user.id);
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
			const result = await changeUserName(getClientFetch(), user.id, { newUserName });
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
			const result = await sendResetLink(getClientFetch(), user.email);
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
								<th>Åtgärder</th>
							</tr>
						</thead>
		<tbody>
			{#each data.users as user (user.id)}
				<tr>
					<td data-label="Användare">{user.userName}</td>
					<td data-label="E-post">{user.email}</td>
					<td data-label="Roller">
						<div class="role-matrix" aria-label={`Roller för ${user.email ?? user.userName}`}>
							{#each data.roles as role}
								{#if hasRole(user, role)}
									<button
										type="button"
										class="role-pill role-pill--assigned"
										disabled={!data.canManageUsers || isSystemAdmin(user) || busyUserId === user.id}
										aria-label={`Ta bort rollen ${role}`}
										on:click={() => removeRole(user, role)}
									>
										{role} Ja
									</button>
								{:else}
									<button
										type="button"
										class="role-pill"
										disabled={!data.canManageUsers || isSystemAdmin(user) || busyUserId === user.id}
										aria-label={`Lägg till rollen ${role}`}
										on:click={() => addRole(user, role)}
									>
										{role} Nej
									</button>
								{/if}
							{/each}
						</div>
					</td>
					<td data-label="Användarnamn">
						<input
							bind:value={userNames[user.id]}
							disabled={!data.canManageUsers || isSystemAdmin(user) || busyUserId === user.id}
							aria-label={`Användarnamn för ${user.email ?? user.userName}`}
						/>
					</td>
					<td data-label="Åtgärder">
						<div class="actions">
							<button type="button" disabled={!data.canManageUsers || isSystemAdmin(user) || busyUserId === user.id} on:click={() => updateUsername(user)}>Byt namn</button>
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
	input:disabled {
		cursor: not-allowed;
		opacity: 0.58;
	}

	.status-text a {
		text-decoration: underline;
		text-underline-offset: 0.2em;
		overflow-wrap: anywhere;
	}

	.role-matrix {
		display: flex;
		flex-wrap: wrap;
		gap: 0.4rem;
		min-width: min(100%, 22rem);
	}

	.role-pill {
		background: rgba(255, 250, 244, 0.64);
		color: var(--color-muted);
	}

	.role-pill--assigned {
		border-color: rgba(143, 162, 132, 0.58);
		background: rgba(143, 162, 132, 0.16);
		color: #586b4f;
	}

	@media (max-width: 760px) {
		:global(.data-table) {
			min-width: 0;
		}

		:global(.data-table thead) {
			display: none;
		}

		:global(.data-table),
		:global(.data-table tbody),
		:global(.data-table tr),
		:global(.data-table td) {
			display: block;
			width: 100%;
		}

		:global(.data-table tr) {
			padding: 1rem;
			border-bottom: 1px solid rgba(95, 74, 59, 0.12);
		}

		:global(.data-table td) {
			display: grid;
			grid-template-columns: minmax(7rem, 0.34fr) minmax(0, 1fr);
			gap: 0.75rem;
			padding: 0.55rem 0;
			border-bottom: 0;
		}

		:global(.data-table td::before) {
			content: attr(data-label);
			color: var(--color-muted);
			font-size: 0.76rem;
			font-weight: 900;
			letter-spacing: 0.05em;
			text-transform: uppercase;
		}

		input {
			width: 100%;
		}
	}

	@media (max-width: 520px) {
		:global(.data-table td) {
			grid-template-columns: 1fr;
			gap: 0.35rem;
		}

		.actions {
			display: grid;
			grid-template-columns: 1fr;
		}
	}
</style>
