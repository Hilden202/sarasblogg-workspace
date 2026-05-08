import { redirect } from '@sveltejs/kit';
import { getAllPosts } from '$lib/services/blogService';
import { getFriendlyApiMessage } from '$lib/api/apiErrors';
import { userHasAnyRole, userHasRole } from '$lib/utils/auth';

export const load = async ({ fetch, parent }) => {
	const { user } = await parent();
	if (!userHasAnyRole(user, ['admin', 'superadmin'])) {
		throw redirect(303, '/admin');
	}

	try {
		const posts = await getAllPosts(fetch);
		return {
			posts,
			error: '',
			canToggleStatus: userHasAnyRole(user, ['admin', 'superadmin']),
			canManagePosts: userHasRole(user, 'superadmin')
		};
	} catch (error) {
		return {
			posts: [],
			error: getFriendlyApiMessage(error, 'Inläggen kunde inte hämtas.'),
			canToggleStatus: userHasAnyRole(user, ['admin', 'superadmin']),
			canManagePosts: userHasRole(user, 'superadmin')
		};
	}
};
