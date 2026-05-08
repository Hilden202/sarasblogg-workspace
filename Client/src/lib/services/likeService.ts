import { apiDelete, apiGet, apiPost, type ApiFetch } from '$lib/api/apiClient';
import type { LikeDto } from '$lib/types/like';

export function getLikeStatus(fetchFn: ApiFetch, bloggId: number) {
	return apiGet<LikeDto>(fetchFn, `/api/likes/${bloggId}`);
}

export function likePost(fetchFn: ApiFetch, bloggId: number) {
	return apiPost<LikeDto>(fetchFn, '/api/likes', { bloggId, userId: '', count: 0, liked: true });
}

export function unlikePost(fetchFn: ApiFetch, bloggId: number) {
	return apiDelete<LikeDto>(fetchFn, `/api/likes/${bloggId}/_`);
}
