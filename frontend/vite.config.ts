import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tsconfigPaths from 'vite-tsconfig-paths';

export default defineConfig({
  plugins: [react(), tsconfigPaths()],
  server: {
    port: 3000,  // Dev server port
    proxy: {
      '/api': {
        target: 'http://localhost:5185',  // Proxy API calls to backend (for local dev without Docker)
        changeOrigin: true,
      },
    },
  },
});