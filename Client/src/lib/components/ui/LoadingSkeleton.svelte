<script lang="ts">
	export let rows = 3;
</script>

<div class="skeleton" aria-hidden="true">
	{#each Array(rows) as _}
		<div></div>
	{/each}
</div>

<style>
	.skeleton {
		display: grid;
		gap: 0.75rem;
	}

	.skeleton div {
		min-height: 1rem;
		border-radius: 999px;
		background: linear-gradient(90deg, rgba(255, 250, 244, 0.72), rgba(244, 217, 202, 0.55), rgba(255, 250, 244, 0.72));
		background-size: 240% 100%;
		animation: shimmer 1.4s ease-in-out infinite;
	}

	.skeleton div:first-child {
		width: 70%;
	}

	.skeleton div:last-child {
		width: 52%;
	}

	@keyframes shimmer {
		to {
			background-position: -240% 0;
		}
	}
</style>
