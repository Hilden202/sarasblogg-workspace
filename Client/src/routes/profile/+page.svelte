<script lang="ts">
	import { goto } from '$app/navigation';
	import ProfileCard from '$lib/components/auth/ProfileCard.svelte';
	import Button from '$lib/components/ui/Button.svelte';
	import FormField from '$lib/components/forms/FormField.svelte';
	import FormSection from '$lib/components/forms/FormSection.svelte';
	import { useClientFetch } from '$lib/api/clientFetch';
	import { getFriendlyApiMessage } from '$lib/api/apiErrors';
	import { getCurrentUser, mapToFrontendUser } from '$lib/services/authService';
	import {
		changeMyUsername,
		deleteMyAccount,
		downloadMyPersonalData,
		updateMyProfile
	} from '$lib/services/userService';
	import { auth } from '$lib/stores/authStore';
	import { toasts } from '$lib/stores/toastStore';
	import { formatDateTime } from '$lib/utils/dates';
	import { routes } from '$lib/utils/routes';

	export let data;

	const getClientFetch = useClientFetch();

	let displayName = data.user.name ?? '';
	let phoneNumber = data.user.phoneNumber ?? '';
	let birthYear = data.user.birthYear ?? null;
	let newUserName = data.user.userName;
	let status = '';
	let error = '';
	let isSaving = false;
	let isExporting = false;
	let isDeleting = false;
	let deleteDialogOpen = false;
	let deletePassword = '';
	let deleteConfirmation = '';

	async function refreshSession() {
		const me = await getCurrentUser(getClientFetch());
		if (me) auth.setUser(mapToFrontendUser(me));
	}

	async function saveProfile() {
		error = '';
		status = '';
		isSaving = true;
		try {
			const result = await updateMyProfile(getClientFetch(), {
				name: displayName || null,
				phoneNumber: phoneNumber || null,
				birthYear
			});
			status = result.message || 'Profilen är uppdaterad.';
			await refreshSession();
			toasts.success(status);
		} catch (err) {
			error = getFriendlyApiMessage(err, 'Profilen kunde inte sparas.');
			toasts.error(error);
		} finally {
			isSaving = false;
		}
	}

	async function saveUsername() {
		error = '';
		status = '';
		try {
			const result = await changeMyUsername(getClientFetch(), { newUserName });
			status = result.message || 'Användarnamnet är uppdaterat.';
			await refreshSession();
			toasts.success(status);
		} catch (err) {
			error = getFriendlyApiMessage(err, 'Användarnamnet kunde inte sparas.');
			toasts.error(error);
		}
	}

	async function exportActivity() {
		error = '';
		status = '';
		isExporting = true;
		try {
			const { blob, filename } = await downloadMyPersonalData(getClientFetch());
			const url = URL.createObjectURL(blob);
			const link = document.createElement('a');
			link.href = url;
			link.download = filename;
			document.body.appendChild(link);
			link.click();
			link.remove();
			setTimeout(() => URL.revokeObjectURL(url), 0);
			toasts.success('Din dataexport laddas ner.');
		} catch (err) {
			error = getFriendlyApiMessage(err, 'Exporten kunde inte hämtas.');
			toasts.error(error);
		} finally {
			isExporting = false;
		}
	}

	function openDeleteDialog() {
		deleteDialogOpen = true;
		deletePassword = '';
		deleteConfirmation = '';
	}

	function closeDeleteDialog() {
		if (isDeleting) return;
		deleteDialogOpen = false;
	}

	async function deleteAccount() {
		if (deleteConfirmation.trim() !== 'RADERA') return;

		error = '';
		status = '';
		isDeleting = true;
		try {
			const result = await deleteMyAccount(getClientFetch(), deletePassword || null);
			auth.clear();
			deleteDialogOpen = false;
			toasts.success(result.message || 'Kontot är raderat.');
			// eslint-disable-next-line svelte/no-navigation-without-resolve
			await goto(routes.home);
		} catch (err) {
			error = getFriendlyApiMessage(err, 'Kontot kunde inte raderas.');
			toasts.error(error);
		} finally {
			isDeleting = false;
		}
	}
</script>

<svelte:head>
	<title>Profil | SarasBlogg</title>
</svelte:head>

<section class="section profile-page">
	<div class="container profile-grid">
		<ProfileCard user={data.user} />

		<FormSection title="Dina uppgifter">
			<form class="form-grid" on:submit|preventDefault={saveProfile}>
				<div class="two-column">
					<FormField label="Namn" id="profile-name">
						<input id="profile-name" bind:value={displayName} />
					</FormField>
					<FormField label="Telefon" id="profile-phone">
						<input id="profile-phone" bind:value={phoneNumber} />
					</FormField>
				</div>
				<FormField label="Födelseår" id="profile-birthyear">
					<input
						id="profile-birthyear"
						type="number"
						min="1900"
						max="2100"
						bind:value={birthYear}
					/>
				</FormField>
				<Button type="submit" disabled={isSaving}>{isSaving ? 'Sparar...' : 'Spara profil'}</Button>
			</form>
		</FormSection>

		<FormSection
			title="Användarnamn"
			text="Om Google-kontot skapade ett temporärt användarnamn kan du byta det här."
		>
			<form class="form-grid" on:submit|preventDefault={saveUsername}>
				<FormField label="Användarnamn" id="profile-username">
					<input id="profile-username" bind:value={newUserName} />
				</FormField>
				<Button type="submit" variant="secondary">Uppdatera</Button>
			</form>
		</FormSection>

		{#if data.personalData}
			<FormSection title="Din aktivitet">
				<div class="activity-grid">
					<div><strong>{data.personalData.commentsCount}</strong><span>Kommentarer</span></div>
					<div><strong>{data.personalData.likesCount}</strong><span>Gillningar</span></div>
				</div>
				<div class="activity-actions">
					<Button
						type="button"
						variant="secondary"
						disabled={isExporting}
						on:click={exportActivity}
					>
						{isExporting ? 'Hämtar...' : 'Ladda ner JSON'}
					</Button>
				</div>
				{#if data.personalData.comments?.length}
					<section class="activity-list" aria-labelledby="profile-comments-title">
						<h3 id="profile-comments-title">Kommentarer</h3>
						{#each data.personalData.comments as comment (comment.id)}
							<!-- eslint-disable-next-line svelte/no-navigation-without-resolve -->
							<a href={`${routes.blog}/${comment.bloggId}`}>
								<strong>{comment.bloggTitle || `Inlägg #${comment.bloggId}`}</strong>
								<span>{formatDateTime(comment.createdAt)}</span>
								<p>{comment.content}</p>
							</a>
						{/each}
					</section>
				{/if}
				{#if data.personalData.likes?.length}
					<section class="activity-list" aria-labelledby="profile-likes-title">
						<h3 id="profile-likes-title">Gillningar</h3>
						{#each data.personalData.likes as like (like.id)}
							<!-- eslint-disable-next-line svelte/no-navigation-without-resolve -->
							<a href={`${routes.blog}/${like.bloggId}`}>
								<strong>{like.bloggTitle || `Inlägg #${like.bloggId}`}</strong>
								<span>{formatDateTime(like.createdAt)}</span>
							</a>
						{/each}
					</section>
				{/if}
			</FormSection>
		{/if}

		<FormSection title="Radera konto">
			<div class="danger-zone">
				<p>Radering tar bort kontot permanent.</p>
				<Button type="button" variant="danger" on:click={openDeleteDialog}>Radera konto</Button>
			</div>
		</FormSection>

		{#if status}
			<p class="status-text status-text--success">{status}</p>
		{/if}
		{#if error}
			<p class="status-text status-text--error">{error}</p>
		{/if}
	</div>
</section>

{#if deleteDialogOpen}
	<div class="delete-backdrop" role="presentation">
		<div
			class="delete-dialog"
			role="dialog"
			aria-modal="true"
			aria-labelledby="delete-account-title"
			aria-describedby="delete-account-message"
		>
			<h2 id="delete-account-title">Är du säker?</h2>
			<p id="delete-account-message">
				Kontot och din åtkomst tas bort permanent. Detta går inte att ångra.
			</p>
			<form class="form-grid" on:submit|preventDefault={deleteAccount}>
				<FormField label="Skriv RADERA för att bekräfta" id="delete-confirmation">
					<input id="delete-confirmation" bind:value={deleteConfirmation} autocomplete="off" />
				</FormField>
				<FormField label="Lösenord" id="delete-password">
					<input
						id="delete-password"
						type="password"
						bind:value={deletePassword}
						autocomplete="current-password"
					/>
				</FormField>
				<div class="delete-actions">
					<Button type="button" variant="ghost" disabled={isDeleting} on:click={closeDeleteDialog}
						>Avbryt</Button
					>
					<button
						type="submit"
						class="delete-submit"
						disabled={deleteConfirmation.trim() !== 'RADERA' || isDeleting}
					>
						{isDeleting ? 'Raderar...' : 'Radera permanent'}
					</button>
				</div>
			</form>
		</div>
	</div>
{/if}

<style>
	.profile-grid {
		display: grid;
		gap: 1.25rem;
	}

	.activity-grid {
		display: grid;
		grid-template-columns: repeat(2, minmax(0, 1fr));
		gap: 1rem;
	}

	.activity-grid div {
		padding: 1rem;
		border-radius: var(--radius-soft);
		background: rgba(244, 217, 202, 0.38);
	}

	.activity-grid strong {
		display: block;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: 2rem;
		line-height: 1;
	}

	.activity-grid span {
		color: var(--color-muted);
		font-weight: 800;
	}

	.activity-actions {
		display: flex;
		justify-content: flex-end;
		margin-top: 1rem;
	}

	.activity-list {
		display: grid;
		gap: 0.65rem;
		margin-top: 1.1rem;
	}

	.activity-list h3 {
		margin: 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: 1.45rem;
	}

	.activity-list a {
		display: grid;
		gap: 0.2rem;
		padding: 0.85rem 0;
		border-top: 1px solid var(--color-border);
	}

	.activity-list strong {
		color: var(--color-heading);
	}

	.activity-list span,
	.activity-list p,
	.danger-zone p,
	.delete-dialog p {
		margin: 0;
		color: var(--color-muted);
	}

	.activity-list p {
		display: -webkit-box;
		-webkit-box-orient: vertical;
		-webkit-line-clamp: 2;
		line-clamp: 2;
		overflow: hidden;
	}

	.danger-zone {
		display: flex;
		flex-wrap: wrap;
		align-items: center;
		justify-content: space-between;
		gap: 1rem;
	}

	.delete-backdrop {
		position: fixed;
		inset: 0;
		z-index: 90;
		display: grid;
		place-items: center;
		padding: 1rem;
		background: rgba(72, 54, 40, 0.32);
		backdrop-filter: blur(5px);
	}

	.delete-dialog {
		width: min(100%, 470px);
		padding: 1.35rem;
		border: 1px solid var(--color-border);
		border-radius: var(--radius-card);
		background: var(--color-surface);
		box-shadow: var(--shadow-soft);
	}

	.delete-dialog h2 {
		margin: 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: 2.1rem;
		line-height: 1.05;
	}

	.delete-dialog form {
		margin-top: 1rem;
	}

	.delete-actions {
		display: flex;
		flex-wrap: wrap;
		justify-content: flex-end;
		gap: 0.65rem;
	}

	.delete-submit {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-height: 2.7rem;
		padding: 0.72rem 1.15rem;
		border: 1px solid transparent;
		border-radius: 999px;
		background: #9b3f35;
		color: #fffaf4;
		font-size: 0.88rem;
		font-weight: 800;
		letter-spacing: 0.04em;
		line-height: 1;
		text-transform: uppercase;
		box-shadow: 0 12px 26px rgba(111, 79, 44, 0.2);
	}

	.delete-submit:disabled {
		cursor: not-allowed;
		opacity: 0.55;
	}

	@media (max-width: 620px) {
		.activity-grid {
			grid-template-columns: 1fr;
		}
	}
</style>
