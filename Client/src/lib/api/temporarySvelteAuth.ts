import { browser } from '$app/environment';

type TemporarySvelteAccessToken = {
	accessToken: string;
	accessTokenExpiresUtc: string;
};

const storageKey = 'sarasblogg.temporarySvelteAccessToken';
let memoryToken: TemporarySvelteAccessToken | null = null;

function isExpired(token: TemporarySvelteAccessToken) {
	const expiresAt = Date.parse(token.accessTokenExpiresUtc);
	return Number.isNaN(expiresAt) || expiresAt <= Date.now();
}

function readStoredToken() {
	if (!browser) return null;

	const raw = sessionStorage.getItem(storageKey);
	if (!raw) return null;

	try {
		const parsed = JSON.parse(raw) as TemporarySvelteAccessToken;
		if (!parsed.accessToken || !parsed.accessTokenExpiresUtc || isExpired(parsed)) {
			clearTemporarySvelteAccessToken();
			return null;
		}

		memoryToken = parsed;
		return parsed;
	} catch {
		clearTemporarySvelteAccessToken();
		return null;
	}
}

export function getTemporarySvelteAccessToken() {
	const token = memoryToken ?? readStoredToken();
	if (!token) return null;

	if (isExpired(token)) {
		clearTemporarySvelteAccessToken();
		return null;
	}

	return token.accessToken;
}

export function setTemporarySvelteAccessToken(
	accessToken: string,
	accessTokenExpiresUtc: string | Date
) {
	const token = {
		accessToken,
		accessTokenExpiresUtc:
			accessTokenExpiresUtc instanceof Date
				? accessTokenExpiresUtc.toISOString()
				: accessTokenExpiresUtc
	};

	memoryToken = token;

	if (browser) {
		// TEMPORARY SVELTE SPA COMPATIBILITY LAYER:
		// The current GitHub Pages + Render deployment makes the API cookie third-party
		// from the Svelte app's point of view. Mobile Safari/WebKit can block or drop
		// that cross-site HttpOnly cookie even when CORS and SameSite=None are correct.
		// Keep only the short-lived access token in memory/sessionStorage so the SPA can
		// send Authorization: Bearer until Svelte moves to same-site hosting, a shared
		// custom domain, or a proper BFF/server-auth flow. Do not persist refresh tokens
		// or move this into long-term localStorage auth.
		sessionStorage.setItem(storageKey, JSON.stringify(token));
	}
}

export function clearTemporarySvelteAccessToken() {
	memoryToken = null;
	if (browser) sessionStorage.removeItem(storageKey);
}
