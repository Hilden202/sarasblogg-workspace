import { redirect } from '@sveltejs/kit';
import { getAllComments } from '$lib/services/commentService';
import { getFriendlyApiMessage } from '$lib/api/apiErrors';
import { userHasAnyRole } from '$lib/utils/auth';
import { routes } from '$lib/utils/routes';

export const load = async ({ fetch, parent }) => {
	const { user } = await parent();
	if (!userHasAnyRole(user, ['superuser', 'admin', 'superadmin'])) {
		throw redirect(303, routes.admin);
	}

	try {
		const comments = await getAllComments(fetch);
		return { comments, error: '' };
	} catch (error) {
		return {
			comments: [],
			error: getFriendlyApiMessage(error, 'Kommentarerna kunde inte hämtas.')
		};
	}
};
