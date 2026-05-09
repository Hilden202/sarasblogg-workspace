import { redirect } from '@sveltejs/kit';
import { getRoles } from '$lib/services/adminService';
import { getFriendlyApiMessage } from '$lib/api/apiErrors';
import { userHasRole } from '$lib/utils/auth';
import { routes } from '$lib/utils/routes';

export const load = async ({ fetch, parent }) => {
	const { user } = await parent();
	if (!userHasRole(user, 'superadmin')) {
		throw redirect(303, routes.admin);
	}

	try {
		const roles = await getRoles(fetch);
		return { roles, error: '' };
	} catch (error) {
		return { roles: [], error: getFriendlyApiMessage(error, 'Roller kunde inte hämtas.') };
	}
};
