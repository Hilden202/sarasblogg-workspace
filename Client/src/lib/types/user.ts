import type { Role } from './auth';

export type UserDto = {
	id: string;
	userName: string;
	email?: string | null;
	name?: string | null;
	birthYear?: number | null;
	emailConfirmed: boolean;
	roles: Role[];
	notifyOnNewPost: boolean;
};

export type PublicUserDto = {
	id: string;
	userName: string;
};

export type CommentPreviewDto = {
	id: number;
	bloggId: number;
	bloggTitle: string;
	content: string;
	createdAt: string;
};

export type LikePreviewDto = {
	id: number;
	bloggId: number;
	bloggTitle: string;
	createdAt: string;
};

export type PersonalDataDto = {
	data: Record<string, string | null>;
	roles: string[];
	claims: { key: string; value: string }[];
	commentsCount: number;
	likesCount: number;
	comments?: CommentPreviewDto[] | null;
	likes?: LikePreviewDto[] | null;
};
