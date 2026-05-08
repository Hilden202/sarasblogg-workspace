import { redirect } from '@sveltejs/kit';
import { getMyPersonalData } from '$lib/services/userService';

export const load = async ({ parent, fetch, url }) => {
	const { user } = await parent();
	if (!user) {
		throw redirect(303, `/login?returnUrl=${encodeURIComponent(url.pathname)}`);
	}

	const personalData = await getMyPersonalData(fetch).catch(() => null);
	return { user, personalData };
};
