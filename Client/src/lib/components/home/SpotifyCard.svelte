<script lang="ts">
	import { spotifyItems } from '$lib/config/site';

	export let items = spotifyItems;

	let active = 0;

	$: current = items[active] ?? items[0];

	function next() {
		active = (active + 1) % items.length;
	}

	function prev() {
		active = (active - 1 + items.length) % items.length;
	}
</script>

{#if current}
	<section class="spotify-card" aria-label="Spotify-spelare">
		<div class="spotify-card__frame">
			<iframe
				title={current.label}
				src={current.embedUrl}
				width="100%"
				height="152"
				frameborder="0"
				allowfullscreen
				allow="autoplay; clipboard-write; encrypted-media; fullscreen; picture-in-picture"
				loading="lazy"
			></iframe>
		</div>

		<div class="spotify-card__controls" aria-label="Byt Spotify-album">
			<button type="button" aria-label="Föregående album" on:click={prev}>‹</button>
			<div class="spotify-card__dots" aria-hidden="true">
				{#each items as item, index}
					<button
						type="button"
						class:is-active={index === active}
						aria-label={`Visa ${item.label}`}
						aria-current={index === active}
						on:click={() => (active = index)}
					></button>
				{/each}
			</div>
			<button type="button" aria-label="Nästa album" on:click={next}>›</button>
			<a href={current.destinationUrl} target="_blank" rel="noreferrer">Öppna i Spotify</a>
		</div>
	</section>
{/if}

<style>
	.spotify-card {
		width: min(100%, 760px);
		margin: 1.4rem auto 0;
		padding: 0.75rem;
		border: 1px solid rgba(217, 155, 121, 0.2);
		border-radius: 1rem;
		background: rgba(255, 250, 244, 0.58);
		box-shadow: var(--shadow-small);
		backdrop-filter: blur(4px);
	}

	.spotify-card__frame {
		overflow: hidden;
		border-radius: 0.85rem;
		background: rgba(112, 85, 63, 0.08);
	}

	iframe {
		display: block;
		border: 0;
	}

	.spotify-card__controls {
		display: flex;
		flex-wrap: wrap;
		align-items: center;
		justify-content: center;
		gap: 0.55rem;
		margin-top: 0.55rem;
	}

	button {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		width: 1.8rem;
		height: 1.8rem;
		padding: 0;
		border: 1px solid rgba(196, 138, 125, 0.28);
		border-radius: 999px;
		background: rgba(255, 255, 255, 0.58);
		color: #6f5a4b;
		font-weight: 900;
		line-height: 1;
	}

	.spotify-card__dots {
		display: flex;
		align-items: center;
		gap: 0.35rem;
	}

	.spotify-card__dots button {
		width: 0.55rem;
		height: 0.55rem;
		border: 0;
		background: rgba(196, 138, 125, 0.32);
	}

	.spotify-card__dots button.is-active {
		background: var(--color-sage);
		transform: scale(1.12);
	}

	a {
		margin-left: 0.35rem;
		color: var(--color-heading);
		font-size: 0.82rem;
		font-weight: 900;
		text-decoration: underline;
		text-underline-offset: 0.25em;
	}

	@media (max-width: 640px) {
		.spotify-card {
			width: min(100vw - 1rem, 760px);
			margin-top: 1rem;
			padding: 0.45rem;
			border-radius: 0.9rem;
		}

		iframe {
			height: 96px;
		}

		a {
			flex-basis: 100%;
			margin-left: 0;
			text-align: center;
		}
	}
</style>
