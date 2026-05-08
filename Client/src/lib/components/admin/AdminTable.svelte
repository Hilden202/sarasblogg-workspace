<script lang="ts">
	export let label = 'Administrativ tabell';
</script>

<div class="table-wrap" role="region" aria-label={label}>
	<table class="data-table">
		<slot />
	</table>
</div>
