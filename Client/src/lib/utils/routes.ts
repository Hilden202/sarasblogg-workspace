import type { BlogPostSummaryDto, BlogPostDetailDto } from '$lib/types/blog';

export const routes = {
	home: '/',
	blog: '/blog',
	about: '/about',
	contact: '/contact',
	login: '/login',
	register: '/register',
	profile: '/profile',
	admin: '/admin',
	adminPosts: '/admin/posts',
	adminComments: '/admin/comments',
	adminAbout: '/admin/about',
	adminUsers: '/admin/users',
	adminRoles: '/admin/roles',
	adminForbiddenWords: '/admin/forbidden-words'
};

export function blogPostPath(post: Pick<BlogPostSummaryDto | BlogPostDetailDto, 'slug' | 'id'>) {
	return `/blog/${post.slug || post.id}`;
}

export function resolveMediaUrl(path?: string | null) {
	if (!path) return '/images/blogg/default.png';
	if (/^https?:\/\//i.test(path)) return path;
	if (path.startsWith('/')) return path;
	return `/${path.replace(/^wwwroot\//, '').replace(/^img\//, 'images/')}`;
}

export function fallbackBlogImage(index = 0) {
	const images = ['/images/blogg/myskaffe.jpg', '/images/blogg/tree.png', '/images/blogg/flowers.jpg'];
	return images[index % images.length];
}
