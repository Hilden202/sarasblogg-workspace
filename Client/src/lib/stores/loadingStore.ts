import { writable } from 'svelte/store';

function createLoadingStore() {
	const { subscribe, update, set } = writable(0);

	return {
		subscribe,
		start: () => update((count) => count + 1),
		stop: () => update((count) => Math.max(0, count - 1)),
		clear: () => set(0)
	};
}

export const loading = createLoadingStore();
