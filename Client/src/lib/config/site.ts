export const brand = {
	name: 'Med Hjärtat som Kompass',
	tagline: 'Inspiration, reflektioner och berättelser från hjärtat.',
	heroLogo: '/images/logo/logga.png',
	compactLogo: '/images/logo/medhjartatsomkompass.png',
	footerFlower: '/images/logo/bottomrightflowercut.png',
	favicon: '/images/logo/hjartafavicon.ico'
};

export const spotifyItems = [
	{
		label: 'Spotify album 1',
		embedUrl: 'https://open.spotify.com/embed/album/4qQjC0R00dOaZm2bxfPnTn?utm_source=generator',
		destinationUrl: 'https://open.spotify.com/album/4qQjC0R00dOaZm2bxfPnTn'
	},
	{
		label: 'Spotify album 2',
		embedUrl: 'https://open.spotify.com/embed/album/3rsAvZCJIOiBzrSshIg7qr?utm_source=generator',
		destinationUrl: 'https://open.spotify.com/album/3rsAvZCJIOiBzrSshIg7qr'
	},
	{
		label: 'Spotify album 3',
		embedUrl: 'https://open.spotify.com/embed/album/0nmtLZ1GkySVEaKjey0zY4?utm_source=generator',
		destinationUrl: 'https://open.spotify.com/album/0nmtLZ1GkySVEaKjey0zY4'
	},
	{
		label: 'Spotify album 4',
		embedUrl: 'https://open.spotify.com/embed/album/50PdYSstjgW7qoP7aOzsDs?utm_source=generator',
		destinationUrl: 'https://open.spotify.com/album/50PdYSstjgW7qoP7aOzsDs'
	}
];

export const spotifyArtistUrl = 'https://open.spotify.com/artist/2QROkiaW3cGKgakL0KpSxa?si=BnoEFVYvSP-4Cjdii2DYYg';

export const socialLinks = [
	{ label: 'Instagram', href: 'https://www.instagram.com/medhjartatsomkompass/', key: 'instagram' },
	{ label: 'Facebook', href: 'https://www.facebook.com/saragussen', key: 'facebook' },
	{ label: 'YouTube', href: 'https://www.youtube.com/@saragustafsson8478', key: 'youtube' },
	{ label: 'Spotify', href: spotifyArtistUrl, key: 'spotify' }
] as const;
