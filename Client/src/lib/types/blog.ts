export type BloggImageDto = {
	id: number;
	bloggId: number;
	filePath: string;
	order: number;
};

export type BlogPostSummaryDto = {
	id: number;
	slug: string;
	title: string;
	showTitle: boolean;
	isTitleGenerated: boolean;
	author: string;
	excerpt: string;
	readingTimeMinutes: number;
	publishedAtUtc: string;
	isArchived: boolean;
	viewCount: number;
	coverImage?: BloggImageDto | null;
};

export type BlogPostDetailDto = {
	id: number;
	slug: string;
	title: string;
	showTitle: boolean;
	isTitleGenerated: boolean;
	content: string;
	author: string;
	readingTimeMinutes: number;
	publishedAtUtc: string;
	isArchived: boolean;
	viewCount: number;
	coverImage?: BloggImageDto | null;
	images: BloggImageDto[];
};

export type BlogPostListDto = {
	page: number;
	pageSize: number;
	totalItems: number;
	totalPages: number;
	items: BlogPostSummaryDto[];
};

export type BlogPostWriteRequest = {
	title?: string | null;
	showTitle?: boolean | null;
	content: string;
	author?: string | null;
	launchDateLocal?: string | null;
	hidden: boolean;
	isArchived: boolean;
};

export type AdminBlogPostDto = {
	id: number;
	title?: string | null;
	showTitle?: boolean;
	isTitleGenerated?: boolean | null;
	content: string;
	author: string;
	images?: BloggImageDto[] | null;
	launchDate: string;
	isArchived: boolean;
	viewCount: number;
	hidden: boolean;
};
