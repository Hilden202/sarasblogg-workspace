<script lang="ts">
	import { afterNavigate, goto } from '$app/navigation';
	import { page } from '$app/stores';
	import BrandLockup from '$lib/components/brand/BrandLockup.svelte';
	import Button from '$lib/components/ui/Button.svelte';
	import MobileMenu from '$lib/components/layout/MobileMenu.svelte';
	import { useClientFetch } from '$lib/api/clientFetch';
	import { logout } from '$lib/services/authService';
	import { auth } from '$lib/stores/authStore';
	import { toasts } from '$lib/stores/toastStore';
	import { isRouteActive, loginRoute, routePathFromUrl, routes } from '$lib/utils/routes';

	const getClientFetch = useClientFetch();

	let open = false;

	afterNavigate(() => {
		open = false;
	});

	const navItems = [
		{ href: routes.home, label: 'Hem' },
		{ href: routes.blog, label: 'Blogg' },
		{ href: routes.about, label: 'Om mig' },
		{ href: routes.contact, label: 'Kontakt' }
	];

	$: path = $page.url.pathname;
	$: loginHref = loginRoute(routePathFromUrl($page.url));
	$: user = $auth.user;
	$: isAdmin =
		user?.roles.some((role) => ['superuser', 'admin', 'superadmin'].includes(role)) ?? false;

	async function handleLogout() {
		try {
			await logout(getClientFetch());
			auth.clear();
			toasts.success('Du är utloggad.');
			open = false;
			await goto(routes.home);
		} catch {
			auth.clear();
			toasts.info('Sessionen är rensad lokalt.');
			await goto(routes.home);
		}
	}
</script>

<header class="site-header">
	<div class="nav-wrap container">
		<BrandLockup variant="nav" href={routes.home} ariaLabel="SarasBlogg startsida" />

		<nav class="desktop-nav" aria-label="Huvudnavigering">
			{#each navItems as item}
				<a class:active={isRouteActive(path, item.href, item.href === routes.home)} href={item.href}
					>{item.label}</a
				>
			{/each}
			{#if isAdmin}
				<a class:active={isRouteActive(path, routes.admin)} href={routes.admin}>Admin</a>
			{/if}
		</nav>

		<div class="desktop-actions">
			{#if user}
				<a class="profile-link" href={routes.profile}>{user.displayName}</a>
				<button
					class="icon-button"
					type="button"
					aria-label="Logga ut"
					title="Logga ut"
					on:click={handleLogout}
				>
					<span aria-hidden="true">↗</span>
				</button>
			{:else}
				<Button href={loginHref} variant="ghost">Logga in</Button>
			{/if}
		</div>

		<button
			class="menu-button"
			type="button"
			aria-label="Öppna meny"
			aria-expanded={open}
			on:click={() => (open = !open)}
		>
			<span class="menu-button__icon" aria-hidden="true"></span>
		</button>

		<MobileMenu
			{open}
			{path}
			userName={user?.displayName ?? null}
			roles={user?.roles ?? []}
			{loginHref}
			onNavigate={() => (open = false)}
			onLogout={handleLogout}
		/>
	</div>
</header>

<style>
	.site-header {
		position: sticky;
		top: 0;
		z-index: 50;
		border-bottom: 1px solid rgba(95, 74, 59, 0.1);
		background: rgba(251, 244, 234, 0.86);
		backdrop-filter: blur(16px);
	}

	.nav-wrap {
		position: relative;
		display: grid;
		grid-template-columns: 1fr auto 1fr;
		align-items: center;
		gap: 1.25rem;
		width: min(100% - 2rem, 1280px);
		max-width: none;
		min-height: 5.7rem;
		margin-inline: auto;
	}

	.nav-wrap :global(.brand-lockup--nav) {
		justify-self: start;
	}

	.desktop-nav {
		display: flex;
		justify-self: center;
		justify-content: center;
		gap: clamp(1.1rem, 3vw, 3rem);
	}

	.desktop-nav a {
		position: relative;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: 1.15rem;
		font-weight: 700;
		letter-spacing: 0.06em;
		text-transform: uppercase;
	}

	.desktop-nav a::after {
		position: absolute;
		right: 0;
		bottom: -0.55rem;
		left: 0;
		height: 2px;
		background: var(--color-accent);
		content: '';
		opacity: 0;
		transform: scaleX(0.5);
		transition:
			opacity 160ms ease,
			transform 160ms ease;
	}

	.desktop-nav a.active::after,
	.desktop-nav a:hover::after {
		opacity: 1;
		transform: scaleX(1);
	}

	.desktop-actions {
		display: flex;
		align-items: center;
		justify-content: flex-end;
		justify-self: end;
		gap: 0.65rem;
	}

	.profile-link {
		color: var(--color-muted);
		font-weight: 800;
	}

	.icon-button,
	.menu-button {
		display: inline-grid;
		place-items: center;
		border: 1px solid var(--color-border);
		border-radius: 0.85rem;
		background: rgba(255, 250, 244, 0.72);
		color: var(--color-heading);
	}

	.icon-button {
		width: 2.55rem;
		height: 2.55rem;
		font-size: 1.1rem;
	}

	.menu-button {
		position: absolute;
		top: 50%;
		right: 0;
		z-index: 5;
		display: inline-flex;
		align-items: center;
		justify-content: center;
		width: 3rem;
		height: 3rem;
		justify-self: end;
		padding: 0;
		transform: translateY(-50%);
	}

	.menu-button__icon,
	.menu-button__icon::before,
	.menu-button__icon::after {
		display: block;
		width: 1.4rem;
		height: 2px;
		border-radius: 999px;
		background: currentColor;
	}

	.menu-button__icon {
		position: relative;
	}

	.menu-button__icon::before,
	.menu-button__icon::after {
		position: absolute;
		left: 0;
		content: '';
	}

	.menu-button__icon::before {
		top: -0.42rem;
	}

	.menu-button__icon::after {
		top: 0.42rem;
	}

	@media (min-width: 1101px) {
		.menu-button {
			display: none;
		}
	}

	@media (max-width: 1100px) {
		.nav-wrap {
			grid-template-columns: minmax(0, 1fr) auto;
			min-height: 5rem;
		}

		.nav-wrap :global(.brand-lockup--nav) {
			min-width: 0;
		}

		.desktop-nav,
		.desktop-actions {
			display: none;
		}

		.menu-button {
			border-color: rgba(95, 74, 59, 0.22);
			background: rgba(255, 250, 244, 0.9);
			color: var(--color-heading);
		}
	}
</style>
