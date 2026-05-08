<script lang="ts">
	import { brand } from '$lib/config/site';

	export let variant: 'nav' | 'hero' | 'footer' = 'nav';
	export let href: string | undefined = undefined;
	export let heading = false;
	export let showTagline = false;
	export let ariaLabel = brand.name;

	const lockupClass = `brand-lockup brand-lockup--${variant}`;
</script>

{#if href}
	<a class={lockupClass} {href} aria-label={ariaLabel}>
		<span class="brand-lockup__mark" aria-hidden="true">
			<img src={brand.heartLogo} alt="" />
		</span>
		<span class="brand-lockup__content">
			<span class="brand-lockup__wordmark" aria-hidden="true">
				<img src={brand.compactLogo} alt="" />
			</span>
			{#if heading}
				<h1>{brand.name}</h1>
			{:else}
				<span class="sr-only">{brand.name}</span>
			{/if}
			{#if showTagline}
				<span class="brand-lockup__tagline">{brand.tagline}</span>
			{/if}
		</span>
	</a>
{:else}
	<div class={lockupClass} aria-label={ariaLabel}>
		<span class="brand-lockup__mark" aria-hidden="true">
			<img src={brand.heartLogo} alt="" />
		</span>
		<span class="brand-lockup__content">
			<span class="brand-lockup__wordmark" aria-hidden="true">
				<img src={brand.compactLogo} alt="" />
			</span>
			{#if heading}
				<h1>{brand.name}</h1>
			{:else}
				<span class="sr-only">{brand.name}</span>
			{/if}
			{#if showTagline}
				<span class="brand-lockup__tagline">{brand.tagline}</span>
			{/if}
		</span>
	</div>
{/if}

<style>
	.brand-lockup {
		--brand-mark-size: 3.2rem;
		--brand-wordmark-width: 9.4rem;
		--brand-gap: 0.42rem;
		--brand-tagline-size: 0.78rem;

		display: inline-flex;
		align-items: center;
		gap: var(--brand-gap);
		color: var(--color-heading);
		text-decoration: none;
	}

	.brand-lockup__mark {
		display: block;
		flex: 0 0 auto;
		width: var(--brand-mark-size);
		height: calc(var(--brand-mark-size) * 1.09);
		padding: var(--brand-mark-padding, 0);
		box-sizing: content-box;
	}

	.brand-lockup__mark img {
		width: 100%;
		height: 100%;
		object-fit: contain;
		filter: drop-shadow(0 0.3rem 0.45rem rgba(217, 155, 121, 0.1));
	}

	.brand-lockup__content {
		display: grid;
		align-content: center;
		justify-items: start;
		min-width: 0;
	}

	.brand-lockup__wordmark {
		position: relative;
		display: block;
		width: var(--brand-wordmark-width);
		aspect-ratio: 2.55 / 1;
		overflow: hidden;
	}

	.brand-lockup__wordmark img {
		position: absolute;
		top: -87.5%;
		left: -4.2%;
		width: 109%;
		max-width: none;
		height: auto;
	}

	h1,
	.sr-only {
		position: absolute;
		width: 1px;
		height: 1px;
		padding: 0;
		margin: -1px;
		overflow: hidden;
		clip: rect(0, 0, 0, 0);
		white-space: nowrap;
		border: 0;
	}

	.brand-lockup__tagline {
		max-width: 20rem;
		margin-top: var(--brand-tagline-gap, -0.15rem);
		color: var(--color-muted);
		font-family: var(--font-serif);
		font-size: var(--brand-tagline-size);
		line-height: 1.35;
	}

	.brand-lockup--nav {
		--brand-mark-size: clamp(3.05rem, 5vw, 3.85rem);
		--brand-wordmark-width: clamp(7.8rem, 13vw, 10.2rem);
		--brand-gap: 0.34rem;
		--brand-mark-padding: 0.05rem;
	}

	.brand-lockup--hero {
		--brand-mark-size: clamp(8.35rem, 18vw, 13.1rem);
		--brand-wordmark-width: clamp(16rem, 38vw, 28rem);
		--brand-gap: clamp(0.7rem, 1.8vw, 1.35rem);
		--brand-tagline-size: clamp(1.05rem, 1.55vw, 1.28rem);
		--brand-tagline-gap: clamp(0.7rem, 1.25vw, 1.05rem);
		--brand-mark-padding: clamp(0.35rem, 0.8vw, 0.7rem);
		align-items: center;
	}

	.brand-lockup--footer {
		--brand-mark-size: clamp(3.55rem, 6.3vw, 4.8rem);
		--brand-wordmark-width: clamp(8.8rem, 15vw, 11.3rem);
		--brand-gap: 0.24rem;
		--brand-tagline-size: 0.86rem;
		--brand-mark-padding: 0.06rem;
	}

	@media (max-width: 620px) {
		.brand-lockup--hero {
			--brand-mark-size: clamp(6.2rem, 33vw, 8.2rem);
			--brand-wordmark-width: clamp(12.75rem, 64vw, 16.25rem);
			--brand-gap: 0.45rem;
			--brand-mark-padding: 0.35rem;
			flex-direction: column;
			text-align: center;
		}

		.brand-lockup--hero .brand-lockup__content {
			justify-items: center;
		}
	}
</style>
