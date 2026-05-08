import { redirect } from '@sveltejs/kit';
import { exchangeExternalLoginCode } from '$lib/services/authService';
import { getFriendlyApiMessage } from '$lib/api/apiErrors';

function safeReturnUrl(value: string | null) {
	if (!value || !value.startsWith('/') || value.startsWith('//')) return '/profile';
	return value;
}

export const load = async ({ url, fetch, cookies }) => {
	const code = url.searchParams.get('code');
	const returnUrl = safeReturnUrl(url.searchParams.get('returnUrl'));

	if (!code) {
		return {
			error: 'Google-inloggningen saknade en giltig kod.'
		};
	}

	try {
		const tokens = await exchangeExternalLoginCode(fetch, code);
		cookies.set('api_access_token', tokens.accessToken, {
			httpOnly: true,
			secure: true,
			sameSite: 'lax',
			path: '/',
			expires: new Date(tokens.accessTokenExpiresUtc)
		});
	} catch (error) {
		return {
			error: getFriendlyApiMessage(error, 'Google-inloggningen kunde inte slutföras.')
		};
	}

	throw redirect(303, returnUrl);
};
