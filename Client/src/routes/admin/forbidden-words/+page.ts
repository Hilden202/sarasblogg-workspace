import { redirect } from '@sveltejs/kit';
import { getForbiddenWords } from '$lib/services/adminService';
import { getFriendlyApiMessage } from '$lib/api/apiErrors';
import { userHasAnyRole } from '$lib/utils/auth';

export const load = async ({ fetch, parent }) => {
	const { user } = await parent();
	if (!userHasAnyRole(user, ['superuser', 'admin', 'superadmin'])) {
		throw redirect(303, '/admin');
	}

	try {
		const words = await getForbiddenWords(fetch);
		return { words, error: '' };
	} catch (error) {
		return { words: [], error: getFriendlyApiMessage(error, 'Förbjudna ord kunde inte hämtas.') };
	}
};
