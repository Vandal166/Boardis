import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tsconfigPaths from 'vite-tsconfig-paths';
import tailwindcss from '@tailwindcss/vite';

export default defineConfig({
  plugins: [react(), tsconfigPaths(), tailwindcss()],
  server: {
    port: 3000,  // Dev server port
    proxy: {
      '/api': {
        target: 'http://localhost:5185',  // Proxy API calls to backend (for local dev without Docker)
        changeOrigin: true,
      },
    },
  },
  build: {
    rollupOptions: {
      output: {
        manualChunks: {
          'react-vendors': ['react', 'react-dom', 'react-router-dom'], // Separate chunk for React and related libraries
          'i18n-vendors': ['i18next', 'react-i18next', 'i18next-browser-languagedetector'], // Separate chunk for i18n libraries
        },
      },
    },
  },
});