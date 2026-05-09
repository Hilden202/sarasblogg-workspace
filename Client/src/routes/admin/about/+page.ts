import { redirect } from '@sveltejs/kit';
import { getAboutMe } from '$lib/services/aboutService';
import { userHasRole } from '$lib/utils/auth';
import { routes } from '$lib/utils/routes';

export const load = async ({ fetch, parent }) => {
	const { user } = await parent();
	if (!userHasRole(user, 'superadmin')) {
		throw redirect(303, routes.admin);
	}

	const about = await getAboutMe(fetch).catch(() => null);
	return { about };
};
