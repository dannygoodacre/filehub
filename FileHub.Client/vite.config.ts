import tailwindcss from '@tailwindcss/vite';
import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vite';
import { readFileSync } from 'fs';

const isDevelopment = process.env.NODE_ENV !== 'production';

export default defineConfig({
    plugins: [sveltekit(), tailwindcss()],
    server: {
        host: '0.0.0.0',
        port: 3001,
        https: isDevelopment ? {
            key: readFileSync('../certs/key.pem'),
            cert: readFileSync('../certs/cert.pem')
        } : undefined
    },
    build: {
        target: 'esnext'
    }
});
