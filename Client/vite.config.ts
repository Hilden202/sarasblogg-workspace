import { defineConfig, loadEnv } from 'vite';
import { sveltekit } from '@sveltejs/kit/vite';

export default defineConfig(({ command, mode }) => {
	const env = loadEnv(mode, process.cwd(), '');
	const apiBaseUrl = env.VITE_API_BASE_URL?.trim();

	if (command === 'build' && !apiBaseUrl) {
		throw new Error(
			'Missing required VITE_API_BASE_URL. Production builds must set it to the deployed API base URL, for example https://sarasbloggapi-backend.onrender.com.'
		);
	}

	return {
		plugins: [sveltekit()],
		server: {
			proxy: {
				'/api': {
					target: apiBaseUrl || 'https://localhost:5003',
					changeOrigin: true,
					secure: false
				}
			}
		}
	};
});
