import type { FrontendUser, Role } from '$lib/types/auth';

export function userHasRole(user: FrontendUser | null | undefined, role: Role) {
	return Boolean(user?.roles.includes(role));
}

export function userHasAnyRole(user: FrontendUser | null | undefined, roles: Role[]) {
	return Boolean(user && roles.some((role) => user.roles.includes(role)));
}
