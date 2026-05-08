import { apiDelete, apiGet, apiPatch, apiPost, apiPut, type ApiFetch } from '$lib/api/apiClient';
import type {
	AdminBlogPostDto,
	BlogPostDetailDto,
	BlogPostListDto,
	BlogPostWriteRequest
} from '$lib/types/blog';

type PublicPostQuery = {
	page?: number;
	pageSize?: number;
	archive?: boolean;
};

function queryString(params: Record<string, string | number | boolean | undefined>) {
	const search = new URLSearchParams();
	for (const [key, value] of Object.entries(params)) {
		if (value !== undefined) search.set(key, String(value));
	}
	const value = search.toString();
	return value ? `?${value}` : '';
}

export function getPublicPosts(fetchFn: ApiFetch, query: PublicPostQuery = {}) {
	return apiGet<BlogPostListDto>(
		fetchFn,
		`/api/blogg/public${queryString({
			page: query.page ?? 1,
			pageSize: query.pageSize ?? 10,
			archive: query.archive ?? false
		})}`
	);
}

export function getPublicPost(fetchFn: ApiFetch, idOrSlug: string, archive = false) {
	return apiGet<BlogPostDetailDto>(fetchFn, `/api/blogg/public/${idOrSlug}${queryString({ archive })}`);
}

export function getAllPosts(fetchFn: ApiFetch) {
	return apiGet<AdminBlogPostDto[]>(fetchFn, '/api/blogg');
}

export function getAdminPost(fetchFn: ApiFetch, id: number) {
	return apiGet<AdminBlogPostDto>(fetchFn, `/api/blogg/${id}`);
}

export function createPost(fetchFn: ApiFetch, request: BlogPostWriteRequest) {
	return apiPost<AdminBlogPostDto>(fetchFn, '/api/blogg', request);
}

export async function updatePost(fetchFn: ApiFetch, id: number, request: BlogPostWriteRequest) {
	await apiPut<void>(fetchFn, `/api/blogg/${id}`, request, { emptyResponse: true });
}

export function togglePostHidden(fetchFn: ApiFetch, id: number) {
	return apiPatch<{ hidden: boolean }>(fetchFn, `/api/blogg/${id}/hidden`);
}

export function togglePostArchived(fetchFn: ApiFetch, id: number) {
	return apiPatch<{ isArchived: boolean }>(fetchFn, `/api/blogg/${id}/archived`);
}

export async function deletePost(fetchFn: ApiFetch, id: number) {
	await apiDelete<void>(fetchFn, `/api/blogg/${id}`, undefined, { emptyResponse: true });
}
