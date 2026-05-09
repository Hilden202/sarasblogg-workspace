import { redirect } from '@sveltejs/kit';
import { getMyPersonalData } from '$lib/services/userService';
import { loginRoute, routePathFromUrl } from '$lib/utils/routes';

export const load = async ({ parent, fetch, url }) => {
	const { user } = await parent();
	if (!user) {
		throw redirect(303, loginRoute(routePathFromUrl(url)));
	}

	const personalData = await getMyPersonalData(fetch).catch(() => null);
	return { user, personalData };
};
