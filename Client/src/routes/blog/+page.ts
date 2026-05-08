import { getPublicPosts } from '$lib/services/blogService';
import { getFriendlyApiMessage } from '$lib/api/apiErrors';

export const load = async ({ fetch, url }) => {
	const page = Number(url.searchParams.get('page') ?? '1');
	const archive = url.searchParams.get('archive') === 'true';

	try {
		const posts = await getPublicPosts(fetch, { page, pageSize: 9, archive });
		return { posts, archive, error: '' };
	} catch (error) {
		return {
			posts: { page, pageSize: 9, totalItems: 0, totalPages: 0, items: [] },
			archive,
			error: getFriendlyApiMessage(error, 'Blogginläggen kunde inte hämtas.')
		};
	}
};
