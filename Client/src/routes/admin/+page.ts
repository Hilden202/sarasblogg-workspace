import { getAllPosts } from '$lib/services/blogService';
import { getAllComments } from '$lib/services/commentService';
import { getUsers } from '$lib/services/userService';

export const load = async ({ fetch }) => {
	const [posts, comments, users] = await Promise.allSettled([
		getAllPosts(fetch),
		getAllComments(fetch),
		getUsers(fetch)
	]);

	return {
		postCount: posts.status === 'fulfilled' ? posts.value.length : null,
		commentCount: comments.status === 'fulfilled' ? comments.value.length : null,
		userCount: users.status === 'fulfilled' ? users.value.length : null
	};
};
