import { env } from '$env/dynamic/public';
import { createApiError } from './apiErrors';

export type ApiFetch = typeof fetch;

export type ApiRequestOptions = Omit<RequestInit, 'body'> & {
	body?: BodyInit | object | null;
	emptyResponse?: boolean;
};

export function getApiBaseUrl() {
	return (env.PUBLIC_API_BASE_URL ?? '').replace(/\/$/, '');
}

export function getExternalApiBaseUrl() {
	return (env.PUBLIC_API_BASE_URL ?? 'https://localhost:5003').replace(/\/$/, '');
}

function resolveApiUrl(path: string) {
	if (/^https?:\/\//i.test(path)) return path;
	const normalizedPath = path.startsWith('/') ? path : `/${path}`;
	return `${getApiBaseUrl()}${normalizedPath}`;
}

async function readResponsePayload(response: Response) {
	if (response.status === 204) return null;

	const contentType = response.headers.get('content-type') ?? '';
	if (contentType.includes('application/json')) {
		return response.json();
	}

	const text = await response.text();
	return text.length > 0 ? text : null;
}

export async function apiRequest<T>(
	fetchFn: ApiFetch,
	path: string,
	options: ApiRequestOptions = {}
): Promise<T> {
	const headers = new Headers(options.headers);
	const hasBody = options.body !== undefined && options.body !== null;
	const isFormData = typeof FormData !== 'undefined' && options.body instanceof FormData;

	if (!headers.has('Accept')) headers.set('Accept', 'application/json');
	if (hasBody && !isFormData && !headers.has('Content-Type')) {
		headers.set('Content-Type', 'application/json');
	}

	const response = await fetchFn(resolveApiUrl(path), {
		...options,
		credentials: options.credentials ?? 'include',
		headers,
		body:
			hasBody && !isFormData && typeof options.body === 'object'
				? JSON.stringify(options.body)
				: (options.body as BodyInit | null | undefined)
	});

	const payload = await readResponsePayload(response);

	if (!response.ok) {
		throw createApiError(response, payload);
	}

	if (options.emptyResponse) return undefined as T;
	return payload as T;
}

export function apiGet<T>(fetchFn: ApiFetch, path: string, options: ApiRequestOptions = {}) {
	return apiRequest<T>(fetchFn, path, { ...options, method: 'GET' });
}

export function apiPost<T>(fetchFn: ApiFetch, path: string, body?: ApiRequestOptions['body'], options: ApiRequestOptions = {}) {
	return apiRequest<T>(fetchFn, path, { ...options, method: 'POST', body });
}

export function apiPut<T>(fetchFn: ApiFetch, path: string, body?: ApiRequestOptions['body'], options: ApiRequestOptions = {}) {
	return apiRequest<T>(fetchFn, path, { ...options, method: 'PUT', body });
}

export function apiPatch<T>(fetchFn: ApiFetch, path: string, body?: ApiRequestOptions['body'], options: ApiRequestOptions = {}) {
	return apiRequest<T>(fetchFn, path, { ...options, method: 'PATCH', body });
}

export function apiDelete<T>(fetchFn: ApiFetch, path: string, body?: ApiRequestOptions['body'], options: ApiRequestOptions = {}) {
	return apiRequest<T>(fetchFn, path, { ...options, method: 'DELETE', body });
}
