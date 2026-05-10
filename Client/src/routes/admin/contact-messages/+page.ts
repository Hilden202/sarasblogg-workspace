import { redirect } from '@sveltejs/kit';
import { getFriendlyApiMessage } from '$lib/api/apiErrors';
import { getContactMessages } from '$lib/services/contactService';
import { userHasAnyRole } from '$lib/utils/auth';
import { routes } from '$lib/utils/routes';

export const load = async ({ fetch, parent }) => {
	const { user } = await parent();
	if (!userHasAnyRole(user, ['admin', 'superadmin'])) {
		throw redirect(303, routes.admin);
	}

	try {
		const messages = await getContactMessages(fetch);
		return {
			messages: messages.sort(
				(a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
			),
			error: ''
		};
	} catch (error) {
		return {
			messages: [],
			error: getFriendlyApiMessage(error, 'Kontaktmeddelanden kunde inte hämtas.')
		};
	}
};
