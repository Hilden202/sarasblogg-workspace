export type ContactRequest = {
	name: string;
	email: string;
	subject: string;
	message: string;
};

export type ContactMessageDto = ContactRequest & {
	id: number;
	createdAt: string;
};
