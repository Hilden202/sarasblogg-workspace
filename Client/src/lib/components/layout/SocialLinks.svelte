<script lang="ts">
	import { socialLinks } from '$lib/config/site';

	const iconPaths = {
		instagram:
			'M7.75 2A5.75 5.75 0 0 0 2 7.75v8.5A5.75 5.75 0 0 0 7.75 22h8.5A5.75 5.75 0 0 0 22 16.25v-8.5A5.75 5.75 0 0 0 16.25 2h-8.5ZM4.5 7.75a3.25 3.25 0 0 1 3.25-3.25h8.5a3.25 3.25 0 0 1 3.25 3.25v8.5a3.25 3.25 0 0 1-3.25 3.25h-8.5a3.25 3.25 0 0 1-3.25-3.25v-8.5Zm7.5 1.25a3.5 3.5 0 1 0 0 7 3.5 3.5 0 0 0 0-7Zm0 1.5a2 2 0 1 1 0 4 2 2 0 0 1 0-4Zm4.38-2.63a.88.88 0 1 0 0 1.75.88.88 0 0 0 0-1.75Z',
		facebook: 'M13.5 9H15l.5-3.5h-2a3 3 0 0 0-3 3v2H9v3h1.5v7h3v-7H15l.5-3H13.5V9Z',
		youtube: 'M10 15.5v-7l6 3.5-6 3.5ZM2 12c0 5.52 4.48 10 10 10s10-4.48 10-10S17.52 2 12 2 2 6.48 2 12Z',
		spotify:
			'M12 2C6.48 2 2 6.48 2 12s4.48 10 10 10 10-4.48 10-10S17.52 2 12 2Zm4.59 14.44a.62.62 0 0 1-.85.2c-2.32-1.42-5.24-1.74-8.68-.95a.62.62 0 0 1-.28-1.21c3.77-.86 7-.49 9.6 1.1.29.18.38.56.21.86Zm1.22-2.71a.77.77 0 0 1-1.06.25c-2.65-1.63-6.69-2.1-9.82-1.15a.77.77 0 1 1-.45-1.47c3.57-1.08 8-.56 11.08 1.34.36.22.47.7.25 1.06Zm.1-2.82c-3.18-1.89-8.43-2.07-11.46-1.15a.92.92 0 1 1-.54-1.75c3.48-1.05 9.27-.85 12.94 1.33a.92.92 0 0 1-.94 1.57Z'
	};
</script>

<div class="social-links" aria-label="Sociala länkar">
	{#each socialLinks as link}
		<a href={link.href} target="_blank" rel="noreferrer" aria-label={link.label} title={link.label}>
			<svg width="22" height="22" viewBox="0 0 24 24" aria-hidden="true" focusable="false">
				<path d={iconPaths[link.key]} />
			</svg>
		</a>
	{/each}
</div>

<style>
	.social-links {
		display: flex;
		flex-wrap: wrap;
		gap: 0.65rem;
	}

	a {
		display: grid;
		place-items: center;
		width: 2.45rem;
		height: 2.45rem;
		border-radius: 999px;
		background: #70553f;
		color: #fffaf4;
		transition:
			transform 160ms ease,
			background-color 160ms ease;
	}

	a:hover {
		background: #8a6248;
		transform: translateY(-1px);
	}

	svg {
		fill: currentColor;
	}
</style>
