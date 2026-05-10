import { redirect } from '@sveltejs/kit';
import { getCurrentUser, mapToFrontendUser } from '$lib/services/authService';
import type { FrontendUser } from '$lib/types/auth';
import { routes } from '$lib/utils/routes';

export const ssr = false;

function isExternalAuthCallbackPath(pathname: string) {
	return pathname.replace(/\/+$/, '').endsWith('/auth/external/callback');
}

function normalizePath(pathname: string) {
	const normalized = pathname.replace(/\/+$/, '');
	return normalized || '/';
}

function isPath(pathname: string, route: string) {
	return normalizePath(pathname) === normalizePath(route);
}

function shouldSkipUsernameSetupRedirect(pathname: string) {
	return (
		isExternalAuthCallbackPath(pathname) ||
		isPath(pathname, routes.profileUsername) ||
		isPath(pathname, routes.login)
	);
}

export const load = async ({ fetch, url }) => {
	if (isExternalAuthCallbackPath(url.pathname)) {
		return {
			user: null,
			isExternalAuthCallback: true
		};
	}

	let user: FrontendUser | null = null;

	try {
		const me = await getCurrentUser(fetch);
		user = me ? mapToFrontendUser(me) : null;
	} catch {
		user = null;
	}

	if (user?.requiresUsernameSetup && !shouldSkipUsernameSetupRedirect(url.pathname)) {
		const returnUrl = `${url.pathname}${url.search}`;
		throw redirect(303, `${routes.profileUsername}?returnUrl=${encodeURIComponent(returnUrl)}`);
	}

	return {
		user,
		isExternalAuthCallback: false
	};
};
