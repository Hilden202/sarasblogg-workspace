<script lang="ts">
	import AdminSidebar from './AdminSidebar.svelte';
</script>

<div class="container admin-layout">
	<AdminSidebar />
	<div class="admin-layout__main">
		<slot />
	</div>
</div>

<style>
	.admin-layout {
		display: grid;
		grid-template-columns: 250px minmax(0, 1fr);
		gap: clamp(1rem, 3vw, 1.75rem);
		padding-top: clamp(1.5rem, 4vw, 3rem);
	}

	.admin-layout__main {
		min-width: 0;
	}

	@media (max-width: 860px) {
		.admin-layout {
			grid-template-columns: 1fr;
		}

		:global(.admin-sidebar) {
			position: static;
		}
	}
</style>
