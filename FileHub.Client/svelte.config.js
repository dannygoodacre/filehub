import adapter from '@sveltejs/adapter-node';
import { vitePreprocess } from '@sveltejs/vite-plugin-svelte';

/** @type {import('@sveltejs/kit').Config} */
const config = {
    preprocess: vitePreprocess({
        postcss: true
    }),

    kit: {
        adapter: adapter({
            out: process.env.WEB_BUILD_DIR
        })
    }
};

export default config;
