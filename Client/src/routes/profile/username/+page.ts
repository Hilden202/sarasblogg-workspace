import { redirect } from '@sveltejs/kit';
import {
	ensureBasePath,
	isSafeLocalPath,
	loginRoute,
	routePathFromUrl,
	routes
} from '$lib/utils/routes';

function normalizePath(path: string) {
	const withoutQuery = path.split('?')[0] ?? '/';
	const normalized = withoutQuery.replace(/\/+$/, '');
	return normalized || '/';
}

function safeReturnUrl(value: string | null) {
	if (!isSafeLocalPath(value)) return routes.profile;

	const returnUrl = ensureBasePath(value);
	if (normalizePath(returnUrl) === normalizePath(routes.profileUsername)) return routes.profile;

	return returnUrl;
}

export const load = async ({ parent, url }) => {
	const { user } = await parent();
	const returnUrl = safeReturnUrl(url.searchParams.get('returnUrl'));

	if (!user) {
		throw redirect(303, loginRoute(routePathFromUrl(url)));
	}

	if (!user.requiresUsernameSetup) {
		throw redirect(303, returnUrl);
	}

	return { user, returnUrl };
};
