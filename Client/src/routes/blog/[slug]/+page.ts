import { error as kitError } from '@sveltejs/kit';
import { ApiError, getFriendlyApiMessage } from '$lib/api/apiErrors';
import { getPublicPost } from '$lib/services/blogService';
import { getCommentsForPost } from '$lib/services/commentService';
import { getLikeStatus } from '$lib/services/likeService';

export const load = async ({ fetch, params, url }) => {
	const archive = url.searchParams.get('archive') === 'true';

	try {
		const post = await getPublicPost(fetch, params.slug, archive);
		const [comments, like] = await Promise.all([
			getCommentsForPost(fetch, post.id).catch(() => []),
			getLikeStatus(fetch, post.id).catch(() => null)
		]);
		return { post, comments, like };
	} catch (error) {
		if (error instanceof ApiError && error.status === 404) {
			throw kitError(404, 'Inlägget hittades inte.');
		}
		throw kitError(500, getFriendlyApiMessage(error, 'Inlägget kunde inte hämtas.'));
	}
};
