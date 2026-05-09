<script lang="ts">
	import Button from '$lib/components/ui/Button.svelte';
	import FormField from '$lib/components/forms/FormField.svelte';
	import { useClientFetch } from '$lib/api/clientFetch';
	import { getFriendlyApiMessage } from '$lib/api/apiErrors';
	import { sendContactMessage } from '$lib/services/contactService';
	import { toasts } from '$lib/stores/toastStore';

	const getClientFetch = useClientFetch();

	let name = '';
	let email = '';
	let subject = '';
	let message = '';
	let isSaving = false;
	let formMessage = '';
	let error = '';

	async function submit() {
		error = '';
		formMessage = '';
		isSaving = true;
		try {
			await sendContactMessage(getClientFetch(), { name, email, subject, message });
			formMessage = 'Tack, ditt meddelande är skickat.';
			toasts.success(formMessage);
			name = '';
			email = '';
			subject = '';
			message = '';
		} catch (err) {
			error = getFriendlyApiMessage(err, 'Meddelandet kunde inte skickas.');
			toasts.error(error);
		} finally {
			isSaving = false;
		}
	}
</script>

<svelte:head>
	<title>Kontakt | SarasBlogg</title>
	<meta name="description" content="Kontakta SarasBlogg." />
</svelte:head>

<section class="section contact-page">
	<div class="container contact-page__grid">
		<div>
			<p class="eyebrow">Kontakt</p>
			<h1>Skriv några rader</h1>
			<p>Här kan du skicka en hälsning, fråga eller tanke. Jag läser allt med värme.</p>
		</div>

		<form class="card" on:submit|preventDefault={submit}>
			<FormField label="Namn" id="contact-name">
				<input id="contact-name" bind:value={name} autocomplete="name" required />
			</FormField>
			<FormField label="E-post" id="contact-email">
				<input id="contact-email" type="email" bind:value={email} autocomplete="email" required />
			</FormField>
			<FormField label="Ämne" id="contact-subject">
				<input id="contact-subject" bind:value={subject} required />
			</FormField>
			<FormField label="Meddelande" id="contact-message">
				<textarea id="contact-message" bind:value={message} rows="7" required></textarea>
			</FormField>
			<Button type="submit" disabled={isSaving}>{isSaving ? 'Skickar...' : 'Skicka'}</Button>
			{#if formMessage}
				<p class="status-text status-text--success">{formMessage}</p>
			{/if}
			{#if error}
				<p class="status-text status-text--error">{error}</p>
			{/if}
		</form>
	</div>
</section>

<style>
	.contact-page__grid {
		display: grid;
		grid-template-columns: 0.9fr 1.1fr;
		gap: clamp(1.5rem, 5vw, 4rem);
		align-items: start;
	}

	h1 {
		margin: 0.25rem 0 1rem;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(3rem, 6vw, 5rem);
		line-height: 0.95;
	}

	.contact-page__grid > div > p:not(.eyebrow) {
		color: var(--color-muted);
	}

	form {
		display: grid;
		gap: 1rem;
		padding: clamp(1.25rem, 3vw, 2rem);
	}

	@media (max-width: 780px) {
		.contact-page__grid {
			grid-template-columns: 1fr;
		}
	}
</style>
