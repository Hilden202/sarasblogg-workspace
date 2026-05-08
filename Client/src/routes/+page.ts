import { getAboutMe } from '$lib/services/aboutService';
import { getPublicPosts } from '$lib/services/blogService';

export const load = async ({ fetch }) => {
	const [postsResult, aboutResult] = await Promise.allSettled([
		getPublicPosts(fetch, { pageSize: 3 }),
		getAboutMe(fetch)
	]);

	return {
		latestPosts: postsResult.status === 'fulfilled' ? postsResult.value.items : [],
		about: aboutResult.status === 'fulfilled' ? aboutResult.value : null
	};
};
