import { apiDelete, apiGet, apiPost, type ApiFetch } from '$lib/api/apiClient';
import type { CommentCreateRequest, CommentDto } from '$lib/types/comment';

export function getCommentsForPost(fetchFn: ApiFetch, bloggId: number) {
	return apiGet<CommentDto[]>(fetchFn, `/api/comment/by-blogg/${bloggId}`);
}

export function getAllComments(fetchFn: ApiFetch) {
	return apiGet<CommentDto[]>(fetchFn, '/api/comment');
}

export function createComment(fetchFn: ApiFetch, request: CommentCreateRequest) {
	return apiPost<CommentDto>(fetchFn, '/api/comment', request);
}

export async function deleteComment(fetchFn: ApiFetch, id: number) {
	await apiDelete<void>(fetchFn, `/api/comment/ById/${id}`, undefined, { emptyResponse: true });
}

export async function deleteCommentsForPost(fetchFn: ApiFetch, bloggId: number) {
	await apiDelete<void>(fetchFn, `/api/comment/ByBlogg/${bloggId}`, undefined, { emptyResponse: true });
}
