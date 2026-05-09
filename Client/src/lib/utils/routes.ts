import { base } from '$app/paths';
import type { BlogPostSummaryDto, BlogPostDetailDto } from '$lib/types/blog';

function appRoute(path: string) {
	const normalizedPath = path.startsWith('/') ? path : `/${path}`;
	return `${base}${normalizedPath}`;
}

export function staticAsset(path: string) {
	const normalizedPath = path.startsWith('/') ? path : `/${path}`;
	const normalizedAssetPath = normalizedPath.replace(/^\/img\//, '/images/');
	return `${base}${normalizedAssetPath}`;
}

function stripBase(pathname: string) {
	if (!base) return pathname || '/';
	if (pathname === base) return '/';
	if (pathname.startsWith(`${base}/`)) return pathname.slice(base.length) || '/';
	return pathname || '/';
}

export function isRouteActive(pathname: string, href: string, exact = false) {
	const currentPath = stripBase(pathname);
	const targetPath = stripBase(href.split('?')[0] || '/');

	if (exact || targetPath === '/') return currentPath === targetPath;
	return currentPath === targetPath || currentPath.startsWith(`${targetPath}/`);
}

export const routes = {
	home: appRoute('/'),
	blog: appRoute('/blogg'),
	about: appRoute('/om-mig'),
	contact: appRoute('/kontakt'),
	login: appRoute('/login'),
	register: appRoute('/register'),
	profile: appRoute('/profile'),
	admin: appRoute('/admin'),
	adminPosts: appRoute('/admin/posts'),
	adminComments: appRoute('/admin/comments'),
	adminAbout: appRoute('/admin/about'),
	adminUsers: appRoute('/admin/users'),
	adminRoles: appRoute('/admin/roles'),
	adminForbiddenWords: appRoute('/admin/forbidden-words')
};

export function blogPostPath(post: Pick<BlogPostSummaryDto | BlogPostDetailDto, 'slug' | 'id'>) {
	return `${routes.blog}/${post.slug || post.id}`;
}

export function resolveMediaUrl(path?: string | null) {
	if (!path) return staticAsset('/images/blogg/default.png');
	if (/^(https?:|data:|blob:)/i.test(path)) return path;
	if (base && (path === base || path.startsWith(`${base}/`))) return path;

	const normalizedPath = path.replace(/^wwwroot\//, '').replace(/^\/?img\//, 'images/');
	const pathWithSlash = normalizedPath.startsWith('/') ? normalizedPath : `/${normalizedPath}`;

	if (/^\/(?:images|favicon|lib)(?:\/|$)/i.test(pathWithSlash)) {
		return staticAsset(pathWithSlash);
	}

	return pathWithSlash;
}

export function fallbackBlogImage(index = 0) {
	const images = [
		staticAsset('/images/blogg/myskaffe.jpg'),
		staticAsset('/images/blogg/tree.png'),
		staticAsset('/images/blogg/flowers.jpg')
	];
	return images[index % images.length];
}
