import { getPublicPosts } from '$lib/services/blogService';

export const prerender = true;

export const load = async ({ fetch }) => {
	try {
		const postsResult = await getPublicPosts(fetch, { pageSize: 3 });
		return {
			latestPosts: postsResult.items
		};
	} catch {
		return {
			latestPosts: []
		};
	}
};
