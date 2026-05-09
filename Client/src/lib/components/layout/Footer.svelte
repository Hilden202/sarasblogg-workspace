<script lang="ts">
	import { createEventDispatcher } from 'svelte';
	import { brand } from '$lib/config/site';
	import BrandLockup from '$lib/components/brand/BrandLockup.svelte';
	import SocialLinks from '$lib/components/layout/SocialLinks.svelte';
	import { routes } from '$lib/utils/routes';

	const dispatch = createEventDispatcher<{ privacy: void }>();
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

		<form class="newsletter" aria-label="Nyhetsbrev" on:submit|preventDefault>
			<h2>Nyhetsbrev</h2>
			<p>Få inspiration och nya inlägg direkt till din inkorg.</p>
			<div>
				<input type="email" placeholder="Din e-postadress" aria-label="Din e-postadress" />
				<button type="submit" aria-label="Anmäl till nyhetsbrev">→</button>
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

	.newsletter button {
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

	.newsletter div {
		display: grid;
		grid-template-columns: 1fr auto;
		gap: 0.5rem;
		margin-top: 1rem;
	}

	.newsletter input {
		min-width: 0;
		border: 1px solid var(--color-border);
		border-radius: 0.75rem;
		background: #fff;
		padding: 0.75rem 0.9rem;
	}

	.newsletter button {
		border: 0;
		background: var(--color-accent);
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
	}
</style>
