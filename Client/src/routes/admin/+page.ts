import { getAllPosts } from '$lib/services/blogService';
import { getAllComments } from '$lib/services/commentService';
import { getContactMessages } from '$lib/services/contactService';
import { getForbiddenWords } from '$lib/services/adminService';
import { getUsers } from '$lib/services/userService';

export const load = async ({ fetch }) => {
	const [posts, comments, users, contactMessages, forbiddenWords] = await Promise.allSettled([
		getAllPosts(fetch),
		getAllComments(fetch),
		getUsers(fetch),
		getContactMessages(fetch),
		getForbiddenWords(fetch)
	]);

	return {
		postCount: posts.status === 'fulfilled' ? posts.value.length : null,
		commentCount: comments.status === 'fulfilled' ? comments.value.length : null,
		userCount: users.status === 'fulfilled' ? users.value.length : null,
		contactMessageCount: contactMessages.status === 'fulfilled' ? contactMessages.value.length : null,
		forbiddenWordCount: forbiddenWords.status === 'fulfilled' ? forbiddenWords.value.length : null
	};
};
