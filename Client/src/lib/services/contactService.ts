import { apiDelete, apiGet, apiPost, type ApiFetch } from '$lib/api/apiClient';
import type { ContactMessageDto, ContactRequest } from '$lib/types/contact';

export function sendContactMessage(fetchFn: ApiFetch, request: ContactRequest) {
	return apiPost<ContactMessageDto>(fetchFn, '/api/contactme', request);
}

export function getContactMessages(fetchFn: ApiFetch) {
	return apiGet<ContactMessageDto[]>(fetchFn, '/api/contactme');
}

export async function deleteContactMessage(fetchFn: ApiFetch, id: number) {
	await apiDelete<void>(fetchFn, `/api/contactme/${id}`, undefined, { emptyResponse: true });
}
