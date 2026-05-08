export type AboutMeDto = {
	id: number;
	title?: string | null;
	content?: string | null;
	image?: string | null;
};

export type AboutMeImageDto = {
	imageUrl?: string | null;
};
