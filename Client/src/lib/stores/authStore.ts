import { writable } from 'svelte/store';
import { clearTemporarySvelteAccessToken } from '$lib/api/temporarySvelteAuth';
import type { FrontendUser, Role } from '$lib/types/auth';

type AuthState = {
	user: FrontendUser | null;
	isLoading: boolean;
};

const roleHierarchy: Record<Role, number> = {
	user: 1,
	superuser: 2,
	admin: 3,
	superadmin: 4
};

function sameUser(a: FrontendUser | null, b: FrontendUser | null) {
	return JSON.stringify(a) === JSON.stringify(b);
}

function createAuthStore() {
	const { subscribe, set, update } = writable<AuthState>({
		user: null,
		isLoading: true
	});

	function setUser(user: FrontendUser | null) {
		update((state) => {
			if (!state.isLoading && sameUser(state.user, user)) return state;
			return { user, isLoading: false };
		});
	}

	function setLoading(isLoading: boolean) {
		update((state) => ({ ...state, isLoading }));
	}

	function clear() {
		clearTemporarySvelteAccessToken();
		set({ user: null, isLoading: false });
	}

	function getCurrentUser(): FrontendUser | null {
		let current: FrontendUser | null = null;
		const unsubscribe = subscribe((state) => {
			current = state.user;
		});
		unsubscribe();
		return current;
	}

	function hasMinRole(required: Role): boolean {
		const user = getCurrentUser();
		if (!user || user.roles.length === 0) return false;

		const highestLevel = Math.max(...user.roles.map((role) => roleHierarchy[role] ?? 0));
		return highestLevel >= roleHierarchy[required];
	}

	function hasRole(role: Role): boolean {
		const user = getCurrentUser();
		return Boolean(user?.roles.includes(role));
	}

	function hasAnyRole(roles: Role[]): boolean {
		const user = getCurrentUser();
		if (!user) return false;
		return roles.some((role) => user.roles.includes(role));
	}

	return {
		subscribe,
		setUser,
		setLoading,
		clear,
		hasRole,
		hasAnyRole,
		hasMinRole
	};
}

export const auth = createAuthStore();
