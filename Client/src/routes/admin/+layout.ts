import { redirect } from '@sveltejs/kit';

const adminRoles = new Set(['superuser', 'admin', 'superadmin']);

export const load = async ({ parent, url }) => {
	const { user } = await parent();

	if (!user) {
		throw redirect(303, `/login?returnUrl=${encodeURIComponent(url.pathname)}`);
	}

	if (!user.roles.some((role) => adminRoles.has(role))) {
		throw redirect(303, '/profile');
	}

	return { user };
};
