import { getCurrentUser, mapToFrontendUser } from '$lib/services/authService';

export const load = async ({ depends, fetch }) => {
	depends('auth:session');

	try {
		const me = await getCurrentUser(fetch);
		return {
			user: me ? mapToFrontendUser(me) : null
		};
	} catch {
		return {
			user: null
		};
	}
};
