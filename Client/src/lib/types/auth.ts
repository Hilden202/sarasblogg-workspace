export type Role = 'user' | 'superuser' | 'admin' | 'superadmin';

export type LoginRequest = {
	userNameOrEmail: string;
	password: string;
	rememberMe: boolean;
};

export type LoginResponse = {
	accessToken: string;
	accessTokenExpiresUtc: string;
	refreshToken: string;
	refreshTokenExpiresUtc: string;
};

export type AccessTokenResponse = {
	accessToken: string;
	accessTokenExpiresUtc: string;
};

export type RegisterRequest = {
	userName: string;
	email: string;
	password: string;
	name?: string | null;
	birthYear?: number | null;
	subscribeNewPosts: boolean;
};

export type BasicResultDto = {
	succeeded: boolean;
	message?: string | null;
	confirmEmailUrl?: string | null;
};

export type AuthSessionDto = {
	id: string;
	userName: string;
	email?: string | null;
	name?: string | null;
	birthYear?: number | null;
	phoneNumber?: string | null;
	emailConfirmed: boolean;
	roles: string[];
	notifyOnNewPost: boolean;
	requiresUsernameSetup: boolean;
};

export type FrontendUser = {
	id: string;
	userName: string;
	email: string;
	emailConfirmed: boolean;
	roles: Role[];
	notifyOnNewPost: boolean;
	requiresUsernameSetup: boolean;
	name?: string | null;
	birthYear?: number | null;
	phoneNumber?: string | null;
};

export type UpdateProfileRequest = {
	phoneNumber?: string | null;
	name?: string | null;
	birthYear?: number | null;
	notifyOnNewPost?: boolean | null;
};

export type ChangeUserNameRequest = {
	newUserName: string;
};
