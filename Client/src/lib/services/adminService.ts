import { apiDelete, apiGet, apiPost, type ApiFetch } from '$lib/api/apiClient';
import type { ForbiddenWordDto } from '$lib/types/admin';

export function getRoles(fetchFn: ApiFetch) {
	return apiGet<string[]>(fetchFn, '/api/role/all');
}

export function createRole(fetchFn: ApiFetch, roleName: string) {
	return apiPost<void>(fetchFn, `/api/role/create/${encodeURIComponent(roleName)}`, undefined, {
		emptyResponse: true
	});
}

export function deleteRole(fetchFn: ApiFetch, roleName: string) {
	return apiDelete<void>(fetchFn, `/api/role/delete/${encodeURIComponent(roleName)}`, undefined, {
		emptyResponse: true
	});
}

export function getForbiddenWords(fetchFn: ApiFetch) {
	return apiGet<ForbiddenWordDto[]>(fetchFn, '/api/forbiddenword');
}

export function createForbiddenWord(fetchFn: ApiFetch, wordPattern: string) {
	return apiPost<ForbiddenWordDto>(fetchFn, '/api/forbiddenword', { wordPattern });
}

export async function deleteForbiddenWord(fetchFn: ApiFetch, id: number) {
	await apiDelete<void>(fetchFn, `/api/forbiddenword/${id}`, undefined, { emptyResponse: true });
}
