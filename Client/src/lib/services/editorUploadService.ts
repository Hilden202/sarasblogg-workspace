import { apiPost, type ApiFetch } from '$lib/api/apiClient';

type EditorImageUploadResponse = {
	location?: string;
	Location?: string;
};

export async function uploadEditorImage(fetchFn: ApiFetch, file: File, bloggId = 0) {
	const formData = new FormData();
	formData.set('file', file);

	const query = bloggId > 0 ? `?bloggId=${bloggId}` : '';
	const response = await apiPost<EditorImageUploadResponse>(fetchFn, `/api/editor/upload-image${query}`, formData);
	const location = response.location ?? response.Location;

	if (!location) {
		throw new Error('Editorbilden laddades upp men saknade URL.');
	}

	return location;
}
