export type CommentDto = {
	id: number;
	bloggId: number;
	name: string;
	content?: string | null;
	createdAt: string;
	topRole?: string | null;
	ownedByCurrentUser: boolean;
	canDelete: boolean;
};

export type CommentCreateRequest = {
	bloggId: number;
	name?: string | null;
	content: string;
};
