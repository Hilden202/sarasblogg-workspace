<script lang="ts">
	import type { FrontendUser } from '$lib/types/auth';
	import { initials } from '$lib/utils/text';

	export let user: FrontendUser;
</script>

<article class="profile-card card">
	<div class="avatar" aria-hidden="true">{initials(user.displayName)}</div>
	<div>
		<p class="eyebrow">Profil</p>
		<h1>{user.displayName}</h1>
		<p>{user.email || user.userName}</p>
		<div class="roles">
			{#each user.roles as role}
				<span class="badge badge--sage">{role}</span>
			{/each}
		</div>
	</div>
</article>

<style>
	.profile-card {
		display: grid;
		grid-template-columns: auto 1fr;
		gap: 1.25rem;
		align-items: center;
		padding: clamp(1.25rem, 4vw, 2rem);
	}

	.avatar {
		display: grid;
		place-items: center;
		width: clamp(4rem, 10vw, 6rem);
		aspect-ratio: 1;
		border-radius: 999px;
		background: rgba(244, 217, 202, 0.76);
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: 2rem;
		font-weight: 800;
	}

	h1 {
		margin: 0;
		color: var(--color-heading);
		font-family: var(--font-serif);
		font-size: clamp(2.2rem, 5vw, 3.4rem);
		line-height: 1;
	}

	p {
		margin: 0.35rem 0 0;
		color: var(--color-muted);
	}

	.roles {
		display: flex;
		flex-wrap: wrap;
		gap: 0.45rem;
		margin-top: 0.9rem;
	}

	@media (max-width: 560px) {
		.profile-card {
			grid-template-columns: 1fr;
			text-align: center;
		}

		.avatar,
		.roles {
			margin-inline: auto;
		}
	}
</style>
