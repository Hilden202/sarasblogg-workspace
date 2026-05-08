<script lang="ts">
	import { page } from '$app/stores';
	import { auth } from '$lib/stores/authStore';
	import type { Role } from '$lib/types/auth';
	import { routes } from '$lib/utils/routes';

	type AdminNavItem = {
		href: string;
		label: string;
		roles: Role[];
	};

	const items: AdminNavItem[] = [
		{ href: routes.admin, label: 'Översikt', roles: ['superuser', 'admin', 'superadmin'] },
		{ href: routes.adminPosts, label: 'Inlägg', roles: ['admin', 'superadmin'] },
		{ href: routes.adminComments, label: 'Kommentarer', roles: ['superuser', 'admin', 'superadmin'] },
		{ href: routes.adminAbout, label: 'Om mig', roles: ['superadmin'] },
		{ href: routes.adminUsers, label: 'Användare', roles: ['admin', 'superadmin'] },
		{ href: routes.adminRoles, label: 'Roller', roles: ['superadmin'] },
		{ href: routes.adminForbiddenWords, label: 'Förbjudna ord', roles: ['superuser', 'admin', 'superadmin'] }
	];

	$: userRoles = $auth.user?.roles ?? [];
	$: visibleItems = items.filter((item) => item.roles.some((role) => userRoles.includes(role)));
</script>

<aside class="admin-sidebar">
	<div>
		<p class="eyebrow">Admin</p>
		<h2>SarasBlogg</h2>
	</div>
	<nav aria-label="Adminnavigering">
		{#each visibleItems as item}
			<a class:active={$page.url.pathname === item.href} href={item.href}>{item.label}</a>
		{/each}
	</nav>
</aside>

<style>
	.admin-sidebar {
		position: sticky;
		top: 7rem;
		display: grid;
		gap: 1.25rem;
		align-self: start;
		padding: 1rem;
		border: 1px solid var(--color-border);
		border-radius: var(--radius-card);
		background: rgba(255, 250, 244, 0.86);
		box-shadow: var(--shadow-small);
	}

	h2 {
		margin: 0.15rem 0 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: 2rem;
	}

	nav {
		display: grid;
		gap: 0.35rem;
	}

	a {
		padding: 0.68rem 0.75rem;
		border-radius: 0.75rem;
		color: var(--color-muted);
		font-weight: 800;
	}

	a:hover,
	a.active {
		background: rgba(244, 217, 202, 0.52);
		color: var(--color-heading);
	}
</style>
