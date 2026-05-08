import { writable } from 'svelte/store';

export type ToastKind = 'success' | 'error' | 'info';

export type ToastMessage = {
	id: number;
	kind: ToastKind;
	message: string;
};

function createToastStore() {
	const { subscribe, update } = writable<ToastMessage[]>([]);
	let nextId = 1;

	function push(message: string, kind: ToastKind = 'info') {
		const id = nextId++;
		update((toasts) => [...toasts, { id, kind, message }]);
		setTimeout(() => dismiss(id), 4500);
		return id;
	}

	function dismiss(id: number) {
		update((toasts) => toasts.filter((toast) => toast.id !== id));
	}

	return {
		subscribe,
		push,
		success: (message: string) => push(message, 'success'),
		error: (message: string) => push(message, 'error'),
		info: (message: string) => push(message, 'info'),
		dismiss
	};
}

export const toasts = createToastStore();
