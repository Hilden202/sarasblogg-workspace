<script lang="ts">
	import ProfileCard from '$lib/components/auth/ProfileCard.svelte';
	import Button from '$lib/components/ui/Button.svelte';
	import FormField from '$lib/components/forms/FormField.svelte';
	import FormSection from '$lib/components/forms/FormSection.svelte';
	import { useClientFetch } from '$lib/api/clientFetch';
	import { getFriendlyApiMessage } from '$lib/api/apiErrors';
	import { getCurrentUser, mapToFrontendUser } from '$lib/services/authService';
	import { changeMyUsername, updateMyProfile } from '$lib/services/userService';
	import { auth } from '$lib/stores/authStore';
	import { toasts } from '$lib/stores/toastStore';

	export let data;

	const getClientFetch = useClientFetch();

	let displayName = data.user.name ?? '';
	let phoneNumber = data.user.phoneNumber ?? '';
	let birthYear = data.user.birthYear ?? null;
	let notifyOnNewPost = data.user.notifyOnNewPost;
	let newUserName = data.user.userName;
	let status = '';
	let error = '';
	let isSaving = false;

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
				birthYear,
				notifyOnNewPost
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
</script>

<svelte:head>
	<title>Profil | SarasBlogg</title>
</svelte:head>

<section class="section profile-page">
	<div class="container profile-grid">
		<ProfileCard user={data.user} />

		<FormSection title="Dina uppgifter" text="Frontend sparar bara DTO-fält som API:t accepterar.">
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
					<input id="profile-birthyear" type="number" min="1900" max="2100" bind:value={birthYear} />
				</FormField>
				<label class="check"><input type="checkbox" bind:checked={notifyOnNewPost} /> Få mejl vid nya inlägg</label>
				<Button type="submit" disabled={isSaving}>{isSaving ? 'Sparar...' : 'Spara profil'}</Button>
			</form>
		</FormSection>

		<FormSection title="Användarnamn" text="Om Google-kontot skapade ett temporärt användarnamn kan du byta det här.">
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
			</FormSection>
		{/if}

		{#if status}
			<p class="status-text status-text--success">{status}</p>
		{/if}
		{#if error}
			<p class="status-text status-text--error">{error}</p>
		{/if}
	</div>
</section>

<style>
	.profile-grid {
		display: grid;
		gap: 1.25rem;
	}

	.check {
		display: inline-flex;
		align-items: center;
		gap: 0.5rem;
		color: var(--color-muted);
		font-weight: 800;
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
</style>
