import { getCurrentUser, mapToFrontendUser } from '$lib/services/authService';

function isExternalAuthCallbackPath(pathname: string) {
	return pathname.replace(/\/+$/, '').endsWith('/auth/external/callback');
}

export const load = async ({ depends, fetch, url }) => {
	if (isExternalAuthCallbackPath(url.pathname)) {
		return {
			user: null,
			isExternalAuthCallback: true
		};
	}

	depends('auth:session');

	try {
		const me = await getCurrentUser(fetch);
		return {
			user: me ? mapToFrontendUser(me) : null,
			isExternalAuthCallback: false
		};
	} catch {
		return {
			user: null,
			isExternalAuthCallback: false
		};
	}
};
