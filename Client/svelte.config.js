import adapter from '@sveltejs/adapter-static';

const dev = process.argv.includes('dev');

/** @type {import('@sveltejs/kit').Config} */
const config = {
	kit: {
		adapter: adapter({
			pages: 'build',
			assets: 'build',
			fallback: '404.html'
		}),

		paths: {
			base: dev ? '' : '/sarasblogg-workspace'
		},

		prerender: {
			entries: ['/', '/om-mig', '/kontakt'],
			handleHttpError: 'warn'
		}
	}
};

export default config;
