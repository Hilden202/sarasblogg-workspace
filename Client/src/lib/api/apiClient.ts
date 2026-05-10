import { createApiError } from './apiErrors';
import { getTemporarySvelteAccessToken } from './temporarySvelteAuth';

export type ApiFetch = typeof fetch;

export type ApiRequestOptions = Omit<RequestInit, 'body'> & {
	body?: BodyInit | object | null;
	emptyResponse?: boolean;
};

const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL ?? '').trim();
const missingApiBaseUrlMessage =
	'Missing required VITE_API_BASE_URL. Production builds must set the API base URL, for example https://sarasbloggapi-backend.onrender.com.';

function normalizeBaseUrl(url: string) {
	return url.replace(/\/+$/, '');
}

export function getApiBaseUrl() {
	if (apiBaseUrl) return normalizeBaseUrl(apiBaseUrl);

	if (import.meta.env.PROD) {
		throw new Error(missingApiBaseUrlMessage);
	}

	return '';
}

export function getExternalApiBaseUrl() {
	if (apiBaseUrl) return normalizeBaseUrl(apiBaseUrl);

	if (import.meta.env.PROD) {
		throw new Error(missingApiBaseUrlMessage);
	}

	return 'https://localhost:5003';
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
	const temporaryAccessToken = getTemporarySvelteAccessToken();
	if (temporaryAccessToken && !headers.has('Authorization')) {
		// TEMPORARY SVELTE SPA COMPATIBILITY LAYER:
		// Mobile Safari/WebKit may reject the Render API's cross-site HttpOnly cookie
		// when Svelte runs on GitHub Pages. Prefer the short-lived access token returned
		// by the existing API login/exchange response for Svelte requests only. Cookie
		// auth stays enabled via credentials: 'include' and Razor remains unchanged.
		// Remove this when Svelte and API are same-site or a BFF/server-auth flow exists.
		headers.set('Authorization', `Bearer ${temporaryAccessToken}`);
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

export async function apiDownload(
	fetchFn: ApiFetch,
	path: string,
	options: ApiRequestOptions = {}
): Promise<Response> {
	const init = { ...options } as RequestInit & { emptyResponse?: boolean };
	delete init.body;
	delete init.emptyResponse;

	const headers = new Headers(options.headers);
	if (!headers.has('Accept')) headers.set('Accept', '*/*');
	const temporaryAccessToken = getTemporarySvelteAccessToken();
	if (temporaryAccessToken && !headers.has('Authorization')) {
		headers.set('Authorization', `Bearer ${temporaryAccessToken}`);
	}

	const response = await fetchFn(resolveApiUrl(path), {
		...init,
		credentials: init.credentials ?? 'include',
		headers,
		method: init.method ?? 'GET'
	});

	if (!response.ok) {
		const payload = await readResponsePayload(response);
		throw createApiError(response, payload);
	}

	return response;
}

export function apiGet<T>(fetchFn: ApiFetch, path: string, options: ApiRequestOptions = {}) {
	return apiRequest<T>(fetchFn, path, { ...options, method: 'GET' });
}

export function apiPost<T>(
	fetchFn: ApiFetch,
	path: string,
	body?: ApiRequestOptions['body'],
	options: ApiRequestOptions = {}
) {
	return apiRequest<T>(fetchFn, path, { ...options, method: 'POST', body });
}

export function apiPut<T>(
	fetchFn: ApiFetch,
	path: string,
	body?: ApiRequestOptions['body'],
	options: ApiRequestOptions = {}
) {
	return apiRequest<T>(fetchFn, path, { ...options, method: 'PUT', body });
}

export function apiPatch<T>(
	fetchFn: ApiFetch,
	path: string,
	body?: ApiRequestOptions['body'],
	options: ApiRequestOptions = {}
) {
	return apiRequest<T>(fetchFn, path, { ...options, method: 'PATCH', body });
}

export function apiDelete<T>(
	fetchFn: ApiFetch,
	path: string,
	body?: ApiRequestOptions['body'],
	options: ApiRequestOptions = {}
) {
	return apiRequest<T>(fetchFn, path, { ...options, method: 'DELETE', body });
}
