import { redirect } from '@sveltejs/kit';
import { routes } from '$lib/utils/routes';

export const load = () => {
	throw redirect(303, routes.login);
};
