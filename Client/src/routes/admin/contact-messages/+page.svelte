<script lang="ts">
	import { invalidateAll } from '$app/navigation';
	import { useClientFetch } from '$lib/api/clientFetch';
	import { getFriendlyApiMessage } from '$lib/api/apiErrors';
	import { deleteContactMessage } from '$lib/services/contactService';
	import { confirmDialog } from '$lib/stores/confirmStore';
	import { toasts } from '$lib/stores/toastStore';
	import { formatDate } from '$lib/utils/dates';
	import type { ContactMessageDto } from '$lib/types/contact';

	export let data;

	const getClientFetch = useClientFetch();
	let selectedMessage: ContactMessageDto | null = data.messages[0] ?? null;
	let busyId: number | null = null;

	$: if (selectedMessage && !data.messages.some((message) => message.id === selectedMessage?.id)) {
		selectedMessage = data.messages[0] ?? null;
	}

	async function remove(message: ContactMessageDto) {
		const confirmed = await confirmDialog.ask({
			title: 'Ta bort kontaktmeddelande',
			message: `Ta bort meddelandet från ${message.name || message.email}?`,
			confirmLabel: 'Ta bort',
			tone: 'danger'
		});
		if (!confirmed) return;

		busyId = message.id;
		try {
			await deleteContactMessage(getClientFetch(), message.id);
			toasts.success('Kontaktmeddelandet togs bort.');
			await invalidateAll();
		} catch (error) {
			toasts.error(getFriendlyApiMessage(error, 'Kontaktmeddelandet kunde inte tas bort.'));
		} finally {
			busyId = null;
		}
	}

	async function removeSelected() {
		if (!selectedMessage) return;
		await remove(selectedMessage);
	}
</script>

<svelte:head>
	<title>Admin · Kontakt | SarasBlogg</title>
</svelte:head>

<section class="admin-page">
	<div>
		<p class="eyebrow">Admin</p>
		<h1>Kontaktmeddelanden</h1>
	</div>

	{#if data.error}
		<p class="status-text status-text--error">{data.error}</p>
	{/if}

	{#if data.messages.length === 0}
		<p class="status-text">Det finns inga kontaktmeddelanden just nu.</p>
	{:else}
		<div class="messages-layout">
			<div class="message-list" aria-label="Kontaktmeddelanden">
				{#each data.messages as message (message.id)}
					<button
						type="button"
						class:active={selectedMessage?.id === message.id}
						on:click={() => (selectedMessage = message)}
					>
						<strong>{message.subject || 'Utan ämne'}</strong>
						<span>{message.name} · {formatDate(message.createdAt)}</span>
					</button>
				{/each}
			</div>

			{#if selectedMessage}
				<article class="message-detail">
					<header>
						<div>
							<p class="eyebrow">{formatDate(selectedMessage.createdAt)}</p>
							<h2>{selectedMessage.subject || 'Utan ämne'}</h2>
						</div>
						<button
							type="button"
							class="danger"
							disabled={busyId === selectedMessage.id}
							on:click={removeSelected}
						>
							Ta bort
						</button>
					</header>

					<dl>
						<div>
							<dt>Namn</dt>
							<dd>{selectedMessage.name}</dd>
						</div>
						<div>
							<dt>E-post</dt>
							<dd><a href={`mailto:${selectedMessage.email}`}>{selectedMessage.email}</a></dd>
						</div>
					</dl>

					<p class="message-body">{selectedMessage.message}</p>
				</article>
			{/if}
		</div>
	{/if}
</section>

<style>
	h1 {
		margin: 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(2.4rem, 5vw, 4rem);
	}

	.messages-layout {
		display: grid;
		grid-template-columns: minmax(220px, 0.38fr) minmax(0, 1fr);
		gap: 1rem;
		align-items: start;
	}

	.message-list {
		display: grid;
		gap: 0.55rem;
	}

	.message-list button,
	.message-detail {
		border: 1px solid var(--color-border);
		border-radius: 0.75rem;
		background: var(--color-surface);
		box-shadow: var(--shadow-small);
	}

	.message-list button {
		display: grid;
		gap: 0.2rem;
		width: 100%;
		padding: 0.85rem;
		color: var(--color-text);
		text-align: left;
	}

	.message-list button.active {
		border-color: rgba(217, 155, 121, 0.62);
		background: rgba(244, 217, 202, 0.36);
	}

	.message-list strong {
		color: var(--color-heading);
		overflow-wrap: anywhere;
	}

	.message-list span {
		color: var(--color-muted);
		font-size: 0.86rem;
	}

	.message-detail {
		display: grid;
		gap: 1rem;
		padding: clamp(1rem, 3vw, 1.35rem);
	}

	header {
		display: flex;
		flex-wrap: wrap;
		gap: 0.85rem;
		align-items: flex-start;
		justify-content: space-between;
	}

	h2 {
		margin: 0.2rem 0 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(1.8rem, 4vw, 2.7rem);
		line-height: 1.05;
		overflow-wrap: anywhere;
	}

	dl {
		display: grid;
		grid-template-columns: repeat(2, minmax(0, 1fr));
		gap: 0.8rem;
		margin: 0;
	}

	dt {
		color: var(--color-muted);
		font-size: 0.76rem;
		font-weight: 900;
		text-transform: uppercase;
	}

	dd {
		margin: 0;
		overflow-wrap: anywhere;
		font-weight: 700;
	}

	a {
		text-decoration: underline;
		text-underline-offset: 0.2em;
	}

	.message-body {
		margin: 0;
		white-space: pre-wrap;
		overflow-wrap: anywhere;
	}

	.danger {
		border: 1px solid rgba(155, 63, 53, 0.32);
		border-radius: 999px;
		background: var(--color-surface);
		color: #9b3f35;
		padding: 0.5rem 0.8rem;
		font-weight: 800;
	}

	.danger:disabled {
		cursor: not-allowed;
		opacity: 0.58;
	}

	@media (max-width: 780px) {
		.messages-layout,
		dl {
			grid-template-columns: 1fr;
		}
	}
</style>
