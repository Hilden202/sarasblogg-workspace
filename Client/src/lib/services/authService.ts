import { apiGet, apiPost, getExternalApiBaseUrl, type ApiFetch } from '$lib/api/apiClient';
import {
	clearTemporarySvelteAccessToken,
	setTemporarySvelteAccessToken
} from '$lib/api/temporarySvelteAuth';
import type {
	AuthSessionDto,
	BasicResultDto,
	FrontendUser,
	LoginResponse,
	RegisterRequest,
	Role
} from '$lib/types/auth';

const knownRoles: Role[] = ['user', 'superuser', 'admin', 'superadmin'];

export function mapToFrontendUser(me: AuthSessionDto): FrontendUser {
	const roles = me.roles
		.map((role) => role.toLowerCase())
		.filter((role): role is Role => knownRoles.includes(role as Role));

	return {
		id: me.id,
		userName: me.userName,
		displayName: me.name?.trim() || me.userName,
		email: me.email ?? '',
		emailConfirmed: me.emailConfirmed,
		roles,
		notifyOnNewPost: me.notifyOnNewPost,
		requiresUsernameSetup: me.requiresUsernameSetup,
		name: me.name,
		birthYear: me.birthYear,
		phoneNumber: me.phoneNumber
	};
}

export async function getCurrentUser(fetchFn: ApiFetch): Promise<AuthSessionDto | null> {
	try {
		return await apiGet<AuthSessionDto>(fetchFn, '/api/users/me');
	} catch (error) {
		if (
			error instanceof Error &&
			'status' in error &&
			(error as { status: number }).status === 401
		) {
			return null;
		}
		throw error;
	}
}

export async function login(
	fetchFn: ApiFetch,
	userNameOrEmail: string,
	password: string,
	rememberMe = true
): Promise<LoginResponse> {
	const response = await apiPost<LoginResponse>(fetchFn, '/api/auth/login', {
		userNameOrEmail,
		password,
		rememberMe
	});
	setTemporarySvelteAccessToken(response.accessToken, response.accessTokenExpiresUtc);
	return response;
}

export async function register(
	fetchFn: ApiFetch,
	request: RegisterRequest
): Promise<BasicResultDto> {
	return apiPost<BasicResultDto>(fetchFn, '/api/auth/register', request);
}

export async function logout(fetchFn: ApiFetch): Promise<void> {
	try {
		await apiPost<void>(fetchFn, '/api/auth/logout', undefined, { emptyResponse: true });
	} finally {
		clearTemporarySvelteAccessToken();
	}
}

export async function exchangeExternalLoginCode(
	fetchFn: ApiFetch,
	code: string
): Promise<LoginResponse> {
	const response = await apiPost<LoginResponse>(fetchFn, '/api/auth/external/exchange', { code });
	setTemporarySvelteAccessToken(response.accessToken, response.accessTokenExpiresUtc);
	return response;
}

export function getGoogleLoginUrl(
	returnUrl: string,
	localReturnUrl = '/',
	callbackPath = '/auth/external/callback/'
) {
	const url = new URL(`${getExternalApiBaseUrl()}/api/auth/external/google/start`);

	url.searchParams.set('returnUrl', returnUrl);
	url.searchParams.set('callbackPath', callbackPath);

	if (localReturnUrl) {
		url.searchParams.set('localReturnUrl', localReturnUrl);
	}

	return url.toString();
}
