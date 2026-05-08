import { apiDelete, apiGet, apiPost, apiPut, type ApiFetch } from '$lib/api/apiClient';
import type { BloggImageDto } from '$lib/types/blog';

export function getBlogImages(fetchFn: ApiFetch, bloggId: number) {
	return apiGet<BloggImageDto[]>(fetchFn, `/api/BloggImage/blogg/${bloggId}`);
}

export function uploadBlogImage(fetchFn: ApiFetch, bloggId: number, file: File) {
	const form = new FormData();
	form.set('file', file);
	form.set('bloggId', String(bloggId));
	return apiPost<BloggImageDto>(fetchFn, '/api/BloggImage/upload', form);
}

export async function updateBlogImageOrder(fetchFn: ApiFetch, bloggId: number, images: BloggImageDto[]) {
	await apiPut<void>(fetchFn, `/api/BloggImage/blogg/${bloggId}/order`, images, {
		emptyResponse: true
	});
}

export async function deleteBlogImage(fetchFn: ApiFetch, imageId: number) {
	await apiDelete<void>(fetchFn, `/api/BloggImage/${imageId}`, undefined, { emptyResponse: true });
}
