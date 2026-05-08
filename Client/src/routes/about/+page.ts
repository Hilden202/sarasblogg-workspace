import { getAboutImage, getAboutMe } from '$lib/services/aboutService';

export const load = async ({ fetch }) => {
	const [aboutResult, imageResult] = await Promise.allSettled([getAboutMe(fetch), getAboutImage(fetch)]);
	return {
		about: aboutResult.status === 'fulfilled' ? aboutResult.value : null,
		image: imageResult.status === 'fulfilled' ? imageResult.value.imageUrl : null
	};
};
