import { redirect } from '@sveltejs/kit';
import { getRoles } from '$lib/services/adminService';
import { getUsers } from '$lib/services/userService';
import { getFriendlyApiMessage } from '$lib/api/apiErrors';
import { userHasAnyRole, userHasRole } from '$lib/utils/auth';
import { routes } from '$lib/utils/routes';

export const load = async ({ fetch, parent }) => {
	const { user } = await parent();
	if (!userHasAnyRole(user, ['admin', 'superadmin'])) {
		throw redirect(303, routes.admin);
	}

	const canManageUsers = userHasRole(user, 'superadmin');
	const [usersResult, rolesResult] = await Promise.allSettled([
		getUsers(fetch),
		canManageUsers ? getRoles(fetch) : Promise.resolve([])
	]);

	return {
		users: usersResult.status === 'fulfilled' ? usersResult.value : [],
		roles: rolesResult.status === 'fulfilled' ? rolesResult.value : [],
		canManageUsers,
		error:
			usersResult.status === 'rejected'
				? getFriendlyApiMessage(usersResult.reason, 'Användare kunde inte hämtas.')
				: ''
	};
};
