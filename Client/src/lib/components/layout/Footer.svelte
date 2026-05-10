<script lang="ts">
	import { createEventDispatcher } from 'svelte';
	import { onMount } from 'svelte';
	import { brand } from '$lib/config/site';
	import { ApiError, getFriendlyApiMessage } from '$lib/api/apiErrors';
	import { useClientFetch } from '$lib/api/clientFetch';
	import BrandLockup from '$lib/components/brand/BrandLockup.svelte';
	import SocialLinks from '$lib/components/layout/SocialLinks.svelte';
	import { requestGuidance } from '$lib/services/guidanceService';
	import { auth } from '$lib/stores/authStore';
	import { routes } from '$lib/utils/routes';

	const dispatch = createEventDispatcher<{ privacy: void }>();
	const getClientFetch = useClientFetch();
	const guestGuidanceKey = 'sarasblogg.footerGuidance.guestUsed';

	let guidanceInput = '';
	let guidanceResponse = '';
	let guidanceError = '';
	let isGuidanceLoading = false;
	let guestHasUsedGuidance = false;

	$: isLoggedIn = Boolean($auth.user);
	$: hasReachedGuestLimit = !isLoggedIn && guestHasUsedGuidance;
	$: guidanceInputValue = guidanceInput.trim();
	$: guidanceSubmitDisabled =
		isGuidanceLoading || guidanceInputValue.length === 0 || hasReachedGuestLimit;

	onMount(() => {
		guestHasUsedGuidance = sessionStorage.getItem(guestGuidanceKey) === 'true';
	});

	function markGuestGuidanceUsed() {
		if (isLoggedIn) return;

		guestHasUsedGuidance = true;
		sessionStorage.setItem(guestGuidanceKey, 'true');
	}

	function getGuidanceErrorMessage(error: unknown) {
		if (error instanceof ApiError && error.status === 429) {
			return 'Vänta en liten stund innan du frågar igen.';
		}

		return getFriendlyApiMessage(
			error,
			'Vägledningen kunde inte hämtas just nu. Försök igen om en stund.'
		);
	}

	async function submitGuidance() {
		if (guidanceSubmitDisabled) return;

		isGuidanceLoading = true;
		guidanceError = '';
		guidanceResponse = '';

		try {
			const result = await requestGuidance(getClientFetch(), { input: guidanceInputValue });
			guidanceResponse = result.guidance;
			guidanceInput = '';
			markGuestGuidanceUsed();
		} catch (error) {
			guidanceError = getGuidanceErrorMessage(error);
		} finally {
			isGuidanceLoading = false;
		}
	}
</script>

<footer class="site-footer">
	<div class="container footer-grid">
		<div class="footer-brand">
			<BrandLockup variant="footer" />
		</div>

		<nav aria-label="Sidfot">
			<h2>Snabblänkar</h2>
			<a href={routes.home}>Hem</a>
			<a href={routes.blog}>Blogg</a>
			<a href={routes.about}>Om mig</a>
			<a href={routes.contact}>Kontakt</a>
		</nav>

		<div>
			<h2>Följ mig</h2>
			<SocialLinks />
		</div>

		<form class="guidance" aria-label="Dagens kompassord" on:submit|preventDefault={submitGuidance}>
			<h2>Dagens kompassord</h2>
			<p>Ställ en fråga eller skriv ett ord, så får du ett litet vägledande svar.</p>
			<div class="guidance-row">
				<input
					type="text"
					bind:value={guidanceInput}
					placeholder="Vad behöver du vägledning kring?"
					aria-label="Vad behöver du vägledning kring?"
					maxlength="180"
					disabled={isGuidanceLoading || hasReachedGuestLimit}
				/>
				<button type="submit" aria-label="Skicka fråga" disabled={guidanceSubmitDisabled}>→</button>
			</div>

			<div class="guidance-status" aria-live="polite">
				{#if isGuidanceLoading}
					<p class="guidance-note guidance-note--loading">Lyssnar in...</p>
				{:else}
					{#if guidanceResponse}
						<p class="guidance-response"><span aria-hidden="true">✦</span>{guidanceResponse}</p>
					{/if}

					{#if guidanceError}
						<p class="guidance-note guidance-note--error">{guidanceError}</p>
					{:else if hasReachedGuestLimit}
						<p class="guidance-note">
							Vill du fråga igen? <a href={routes.login}>Logga in</a> så finns kompassordet kvar.
						</p>
					{/if}
				{/if}
			</div>
		</form>
	</div>

	<div class="container footer-bottom">
		<span>© 2026 {brand.name}</span>
		<button type="button" on:click={() => dispatch('privacy')}>Integritet · Cookies</button>
	</div>
	<img class="footer-flower" src={brand.footerFlower} alt="" loading="lazy" />
</footer>

<style>
	.site-footer {
		margin-top: clamp(3rem, 7vw, 6rem);
		padding: clamp(2.5rem, 5vw, 4rem) 0 1.5rem;
		border-top: 1px solid rgba(95, 74, 59, 0.1);
		background: rgba(255, 250, 244, 0.7);
		position: relative;
		overflow: hidden;
	}

	.footer-grid {
		position: relative;
		z-index: 2;
		display: grid;
		grid-template-columns: 1.4fr 0.8fr 0.8fr 1.4fr;
		gap: 2rem;
		align-items: start;
	}

	.footer-brand {
		display: flex;
		align-items: center;
	}

	h2 {
		margin: 0 0 0.85rem;
		color: var(--color-heading);
		font-size: 0.82rem;
		font-weight: 900;
		letter-spacing: 0.08em;
		text-transform: uppercase;
	}

	p {
		margin: 0;
		color: var(--color-muted);
		max-width: 18rem;
	}

	nav {
		display: grid;
		gap: 0.38rem;
	}

	nav a {
		color: var(--color-muted);
	}

	.guidance button {
		display: grid;
		place-items: center;
		width: 2.45rem;
		height: 2.45rem;
		border-radius: 999px;
		background: #70553f;
		color: #fffaf4;
		font-weight: 900;
	}

	.footer-flower {
		position: absolute;
		right: 0;
		bottom: 0;
		width: clamp(90px, 12vw, 180px);
		pointer-events: none;
		opacity: 0.86;
		z-index: 1;
	}

	.guidance-row {
		display: grid;
		grid-template-columns: 1fr auto;
		gap: 0.5rem;
		margin-top: 1rem;
	}

	.guidance input {
		min-width: 0;
		border: 1px solid var(--color-border);
		border-radius: 0.75rem;
		background: #fff;
		padding: 0.75rem 0.9rem;
		color: var(--color-text);
	}

	.guidance input:disabled {
		background: rgba(255, 250, 244, 0.7);
		color: var(--color-muted);
	}

	.guidance button {
		border: 0;
		background: var(--color-accent);
		transition:
			transform 160ms ease,
			opacity 160ms ease,
			background 160ms ease;
	}

	.guidance button:not(:disabled):hover {
		transform: translateX(1px);
		background: #c98277;
	}

	.guidance button:disabled {
		cursor: default;
		opacity: 0.52;
	}

	.guidance-status {
		display: grid;
		gap: 0.65rem;
		min-height: 0;
		margin-top: 1rem;
	}

	.guidance-response,
	.guidance-note {
		margin: 0;
		max-width: none;
	}

	.guidance-response {
		display: grid;
		grid-template-columns: auto 1fr;
		gap: 0.72rem;
		align-items: start;
		padding: 0.9rem 1rem;
		border: 1px solid rgba(217, 155, 121, 0.22);
		border-radius: 0.9rem;
		background: rgba(255, 250, 244, 0.78);
		box-shadow: 0 10px 28px rgba(95, 74, 59, 0.07);
		color: var(--color-heading);
		line-height: 1.55;
	}

	.guidance-response span {
		color: var(--color-accent);
		font-size: 1.1rem;
		line-height: 1.45;
	}

	.guidance-note {
		color: var(--color-muted);
		font-size: 0.92rem;
		line-height: 1.5;
	}

	.guidance-note a {
		color: #9f664f;
		font-weight: 700;
		text-decoration: underline;
		text-underline-offset: 0.2em;
	}

	.guidance-note--loading {
		animation: soft-pulse 1.4s ease-in-out infinite;
	}

	.guidance-note--error {
		color: #9b3f35;
	}

	.footer-bottom {
		position: relative;
		z-index: 2;
		display: flex;
		justify-content: space-between;
		gap: 1rem;
		margin-top: 2rem;
		padding-top: 1rem;
		border-top: 1px solid rgba(95, 74, 59, 0.09);
		color: var(--color-muted);
		font-size: 0.85rem;
	}

	.footer-bottom button {
		padding: 0;
		border: 0;
		background: transparent;
		color: inherit;
		font: inherit;
		cursor: pointer;
	}

	.footer-bottom button:hover {
		color: var(--color-heading);
		text-decoration: underline;
		text-underline-offset: 0.22em;
	}

	@media (max-width: 860px) {
		.footer-grid {
			grid-template-columns: 1fr 1fr;
		}
	}

	@media (max-width: 560px) {
		.footer-grid,
		.footer-bottom {
			grid-template-columns: 1fr;
			flex-direction: column;
		}

		.guidance-row {
			grid-template-columns: minmax(0, 1fr) auto;
		}
	}

	@keyframes soft-pulse {
		0%,
		100% {
			opacity: 0.62;
		}

		50% {
			opacity: 1;
		}
	}
</style>
