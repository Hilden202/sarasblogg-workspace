import { redirect } from '@sveltejs/kit';
import { loginRoute, routePathFromUrl, routes } from '$lib/utils/routes';

const adminRoles = new Set(['superuser', 'admin', 'superadmin']);

export const load = async ({ parent, url }) => {
	const { user } = await parent();

	if (!user) {
		throw redirect(303, loginRoute(routePathFromUrl(url)));
	}

	if (!user.roles.some((role) => adminRoles.has(role))) {
		throw redirect(303, routes.profile);
	}

	return { user };
};
