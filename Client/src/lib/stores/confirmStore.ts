import { writable } from 'svelte/store';

export type ConfirmTone = 'default' | 'danger';

export type ConfirmOptions = {
	title?: string;
	message: string;
	confirmLabel?: string;
	cancelLabel?: string;
	tone?: ConfirmTone;
};

type ConfirmState = Required<ConfirmOptions> & {
	resolve: (confirmed: boolean) => void;
};

function createConfirmStore() {
	const { subscribe, set, update } = writable<ConfirmState | null>(null);

	function ask(options: ConfirmOptions) {
		return new Promise<boolean>((resolve) => {
			set({
				title: options.title ?? 'Bekräfta',
				message: options.message,
				confirmLabel: options.confirmLabel ?? 'Fortsätt',
				cancelLabel: options.cancelLabel ?? 'Avbryt',
				tone: options.tone ?? 'default',
				resolve
			});
		});
	}

	function answer(confirmed: boolean) {
		update((state) => {
			state?.resolve(confirmed);
			return null;
		});
	}

	return {
		subscribe,
		ask,
		answer
	};
}

export const confirmDialog = createConfirmStore();
