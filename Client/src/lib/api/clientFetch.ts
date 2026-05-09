import { onMount } from 'svelte';
import type { ApiFetch } from './apiClient';

export function useClientFetch() {
	let clientFetch: ApiFetch | null = null;

	onMount(() => {
		clientFetch = window.fetch.bind(window);
	});

	return () => {
		if (!clientFetch) {
			throw new Error('Client fetch is only available after the component has mounted.');
		}

		return clientFetch;
	};
}
