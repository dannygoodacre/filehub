import { resolve } from 'path';

import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

import type { UserConfigExport } from 'vitest/config';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3001
  },
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src'),
      '@styles': resolve(__dirname, 'src/styles')
    }
  },
  test: {
    globals: true,
    isolate: true,
    environment: 'jsdom',
    setupFiles: './setup_vitest.ts',
    css: true
  }
} as UserConfigExport);
