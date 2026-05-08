<script lang="ts">
	import Button from '$lib/components/ui/Button.svelte';
	import type { Role } from '$lib/types/auth';
	import { routes } from '$lib/utils/routes';

	export let open = false;
	export let path = '/';
	export let userName: string | null = null;
	export let roles: Role[] = [];
	export let onNavigate: () => void = () => {};
	export let onLogout: () => void | Promise<void> = () => {};

	const items = [
		{ href: routes.home, label: 'Hem' },
		{ href: routes.blog, label: 'Blogg' },
		{ href: routes.about, label: 'Om mig' },
		{ href: routes.contact, label: 'Kontakt' }
	];

	$: isAdmin = roles.some((role) => ['superuser', 'admin', 'superadmin'].includes(role));
</script>

{#if open}
	<div class="mobile-panel">
		<nav aria-label="Mobil navigering">
			{#each items as item}
				<a class:active={path === item.href || (item.href !== '/' && path.startsWith(item.href))} href={item.href} on:click={onNavigate}>{item.label}</a>
			{/each}
			{#if isAdmin}
				<a class:active={path.startsWith(routes.admin)} href={routes.admin} on:click={onNavigate}>Admin</a>
			{/if}
		</nav>

		<div class="mobile-panel__actions">
			{#if userName}
				<a href={routes.profile} on:click={onNavigate}>{userName}</a>
				<button type="button" on:click={onLogout}>Logga ut</button>
			{:else}
				<Button href={routes.login} variant="secondary" full>Logga in</Button>
				<Button href={routes.register} full>Skapa konto</Button>
			{/if}
		</div>
	</div>
{/if}

<style>
	.mobile-panel {
		position: absolute;
		inset: calc(100% + 0.65rem) 1rem auto;
		z-index: 40;
		display: grid;
		gap: 1rem;
		padding: 1rem;
		border: 1px solid var(--color-border);
		border-radius: var(--radius-card);
		background: rgba(255, 250, 244, 0.96);
		box-shadow: var(--shadow-soft);
	}

	nav,
	.mobile-panel__actions {
		display: grid;
		gap: 0.45rem;
	}

	a,
	button {
		width: 100%;
		padding: 0.75rem 0.85rem;
		border: 0;
		border-radius: 0.8rem;
		background: transparent;
		color: var(--color-heading);
		font-weight: 800;
		text-align: left;
	}

	a.active,
	a:hover,
	button:hover {
		background: rgba(244, 217, 202, 0.5);
	}

	@media (min-width: 861px) {
		.mobile-panel {
			display: none;
		}
	}
</style>
