<script lang="ts">
	import { browser } from '$app/environment';
	import { onMount } from 'svelte';
	import Button from '$lib/components/ui/Button.svelte';
	import Modal from '$lib/components/ui/Modal.svelte';

	const consentStorageKey = 'sarasblogg.cookieConsent.v1';

	let isBannerVisible = false;
	let isPrivacyOpen = false;

	onMount(() => {
		isBannerVisible = !hasConsent();
	});

	function hasConsent() {
		return browser && localStorage.getItem(consentStorageKey) === 'accepted';
	}

	function acceptConsent() {
		if (browser) {
			localStorage.setItem(consentStorageKey, 'accepted');
		}
		isBannerVisible = false;
		isPrivacyOpen = false;
	}

	export function openPrivacy() {
		isPrivacyOpen = true;
	}

	function closePrivacy() {
		isPrivacyOpen = false;
	}
</script>

{#if isBannerVisible}
	<section class="cookie-banner" aria-labelledby="cookie-banner-title">
		<div>
			<p id="cookie-banner-title" class="cookie-banner__title">Vår cookie-policy</p>
			<p>
				Vi använder endast nödvändiga cookies och lokal lagring för att webbplatsen ska fungera
				korrekt. Genom att klicka på Acceptera godkänner du detta och slipper se rutan igen.
			</p>
		</div>
		<div class="cookie-banner__actions">
			<button type="button" class="text-button" on:click={openPrivacy}>Läs mer</button>
			<Button type="button" on:click={acceptConsent}>Acceptera</Button>
		</div>
	</section>
{/if}

<Modal open={isPrivacyOpen} title="Integritet och cookies" on:close={closePrivacy}>
	<div class="privacy-content">
		<p>
			På SarasBlogg värnar vi om din integritet. Den här sidan förklarar hur vi hanterar
			personuppgifter, cookies och annan information kopplad till din användning av webbplatsen.
			Vi sparar inga känsliga uppgifter och samlar inte in mer data än nödvändigt.
		</p>

		<h3>Cookies och lokal lagring</h3>
		<p>
			Cookies används enbart för att förbättra upplevelsen, till exempel för att komma ihåg
			om du accepterat cookies eller är inloggad. I Svelte-versionen sparas cookie-banner-valet
			i webbläsarens lokala lagring.
		</p>

		<table>
			<thead>
				<tr>
					<th>Namn</th>
					<th>Syfte</th>
					<th>Leverantör</th>
				</tr>
			</thead>
			<tbody>
				<tr>
					<td>{consentStorageKey}</td>
					<td>Kommer ihåg att du accepterat cookieinformationen.</td>
					<td>SarasBlogg</td>
				</tr>
				<tr>
					<td>SarasAuth / api_access_token</td>
					<td>Nödvändiga inloggnings- och åtkomstcookies när du loggar in.</td>
					<td>SarasBlogg</td>
				</tr>
			</tbody>
		</table>

		<h3>Kommentarer och konto</h3>
		<p>
			Om du lämnar kommentarer eller skapar ett konto sparas din information endast i syfte
			att driva bloggen och kommer inte att delas med tredje part.
		</p>

		<h3>Tredje parter</h3>
		<p>
			Vi använder Googles tjänster för kommentarsmoderering via Perspective API och för extern
			inloggning när du väljer Google OAuth. Då behandlas uppgifter enligt
			<a href="https://policies.google.com/privacy" target="_blank" rel="noopener">Googles integritetspolicy</a>.
		</p>

		<h3>Dina rättigheter</h3>
		<p>
			Du kan ladda ned eller radera din personliga data via kontosidorna. Nödvändiga cookies
			används med berättigat intresse för drift och säkerhet, och kommentarsmoderering används
			för att motverka missbruk och spam. Om du rensar webbläsarens lagring visas bannern igen
			vid nästa besök.
		</p>

		<div class="privacy-content__actions">
			<Button type="button" on:click={acceptConsent}>Acceptera</Button>
			<Button type="button" variant="ghost" on:click={closePrivacy}>Stäng</Button>
		</div>
	</div>
</Modal>

<style>
	.cookie-banner {
		position: fixed;
		right: 1rem;
		bottom: 1rem;
		left: 1rem;
		z-index: 75;
		display: grid;
		grid-template-columns: minmax(0, 1fr) auto;
		gap: 1rem;
		align-items: center;
		width: min(920px, calc(100vw - 2rem));
		margin-inline: auto;
		padding: 1rem;
		border: 1px solid rgba(217, 155, 121, 0.5);
		border-radius: var(--radius-card);
		background: rgba(255, 250, 244, 0.96);
		box-shadow: var(--shadow-soft);
		backdrop-filter: blur(14px);
	}

	.cookie-banner__title {
		margin: 0 0 0.25rem;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: 1.55rem;
		font-weight: 700;
		line-height: 1.05;
	}

	.cookie-banner p {
		margin: 0;
		color: var(--color-muted);
	}

	.cookie-banner__actions,
	.privacy-content__actions {
		display: flex;
		flex-wrap: wrap;
		gap: 0.65rem;
		align-items: center;
		justify-content: flex-end;
	}

	.text-button {
		border: 0;
		background: transparent;
		color: #9f664f;
		font-weight: 800;
		text-decoration: underline;
		text-underline-offset: 0.22em;
	}

	.privacy-content {
		display: grid;
		gap: 1rem;
		color: var(--color-text);
	}

	.privacy-content p,
	.privacy-content h3 {
		margin: 0;
	}

	.privacy-content h3 {
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: 1.45rem;
		line-height: 1.1;
	}

	table {
		display: block;
		width: 100%;
		max-width: 100%;
		overflow-x: auto;
		border-collapse: collapse;
		border: 1px solid var(--color-border);
		border-radius: var(--radius-soft);
	}

	th,
	td {
		padding: 0.7rem 0.8rem;
		border-bottom: 1px solid rgba(95, 74, 59, 0.1);
		text-align: left;
		vertical-align: top;
	}

	th {
		color: var(--color-muted);
		font-size: 0.76rem;
		font-weight: 900;
		letter-spacing: 0.06em;
		text-transform: uppercase;
	}

	tr:last-child td {
		border-bottom: 0;
	}

	@media (max-width: 720px) {
		.cookie-banner {
			grid-template-columns: 1fr;
			bottom: 0.5rem;
			max-height: min(52dvh, 24rem);
			overflow: auto;
		}

		.cookie-banner__actions,
		.privacy-content__actions {
			justify-content: flex-start;
		}
	}
</style>
