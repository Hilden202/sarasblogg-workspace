import { apiDelete, apiGet, apiPost, apiPut, type ApiFetch } from '$lib/api/apiClient';
import type { AboutMeDto, AboutMeImageDto } from '$lib/types/about';

export function getAboutMe(fetchFn: ApiFetch) {
	return apiGet<AboutMeDto>(fetchFn, '/api/aboutme');
}

export function getAboutImage(fetchFn: ApiFetch) {
	return apiGet<AboutMeImageDto>(fetchFn, '/api/aboutme/image');
}

export function uploadAboutImage(fetchFn: ApiFetch, file: File) {
	const form = new FormData();
	form.set('file', file);
	return apiPut<AboutMeImageDto>(fetchFn, '/api/aboutme/image', form);
}

export async function deleteAboutImage(fetchFn: ApiFetch) {
	await apiDelete<void>(fetchFn, '/api/aboutme/image', undefined, { emptyResponse: true });
}

export function createAboutMe(fetchFn: ApiFetch, about: Omit<AboutMeDto, 'id'>) {
	return apiPost<AboutMeDto>(fetchFn, '/api/aboutme', about);
}

export async function updateAboutMe(fetchFn: ApiFetch, about: AboutMeDto) {
	await apiPut<void>(
		fetchFn,
		`/api/aboutme/${about.id}`,
		{
			id: about.id,
			title: about.title,
			content: about.content,
			image: about.image
		},
		{ emptyResponse: true }
	);
}

export async function deleteAboutMe(fetchFn: ApiFetch, id: number) {
	await apiDelete<void>(fetchFn, `/api/aboutme/${id}`, undefined, { emptyResponse: true });
}
