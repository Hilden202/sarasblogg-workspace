export type ApiFieldErrors = Record<string, string[]>;

export class ApiError extends Error {
	status: number;
	statusText: string;
	details?: unknown;
	fieldErrors: ApiFieldErrors;

	constructor(message: string, status: number, statusText = '', details?: unknown, fieldErrors: ApiFieldErrors = {}) {
		super(message);
		this.name = 'ApiError';
		this.status = status;
		this.statusText = statusText;
		this.details = details;
		this.fieldErrors = fieldErrors;
	}

	get isUnauthorized() {
		return this.status === 401;
	}

	get isForbidden() {
		return this.status === 403;
	}
}

type ProblemDetails = {
	title?: string;
	detail?: string;
	message?: string;
	errors?: ApiFieldErrors | Record<string, string | string[]>;
};

function normalizeFieldErrors(errors: ProblemDetails['errors']): ApiFieldErrors {
	if (!errors) return {};

	return Object.entries(errors).reduce<ApiFieldErrors>((acc, [key, value]) => {
		acc[key] = Array.isArray(value) ? value.map(String) : [String(value)];
		return acc;
	}, {});
}

export function createApiError(response: Response, payload: unknown): ApiError {
	if (typeof payload === 'string' && payload.trim()) {
		return new ApiError(payload, response.status, response.statusText, payload);
	}

	if (payload && typeof payload === 'object') {
		const problem = payload as ProblemDetails;
		const message =
			problem.message ??
			problem.detail ??
			problem.title ??
			(response.status === 401
				? 'Du behöver logga in igen.'
				: response.status === 403
					? 'Du har inte behörighet att göra detta.'
					: 'Något gick fel vid kontakten med servern.');

		return new ApiError(message, response.status, response.statusText, payload, normalizeFieldErrors(problem.errors));
	}

	return new ApiError(
		response.status === 401
			? 'Du behöver logga in igen.'
			: response.status === 403
				? 'Du har inte behörighet att göra detta.'
				: 'Något gick fel vid kontakten med servern.',
		response.status,
		response.statusText,
		payload
	);
}

export function getFriendlyApiMessage(error: unknown, fallback = 'Något gick fel. Försök igen om en stund.') {
	if (error instanceof ApiError) return error.message;
	if (error instanceof Error && error.message) return error.message;
	return fallback;
}
