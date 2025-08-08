import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";

export default defineConfig(({ mode }) => {
    // Load all env variables for the current mode
    const env = loadEnv(mode, process.cwd(), "");

    // Filter and expose only REACT_APP_ variables
    const envVars = Object.keys(env)
        .filter((key) => key.startsWith("REACT_APP_"))
        .reduce((prev, key) => {
            prev[`import.meta.env.${key}`] = JSON.stringify(env[key]);
            return prev;
        }, {} as Record<string, string>);

    return {
        plugins: [react()],
        define: {
            ...envVars,
        },
        resolve: {
            alias: {
                "@": path.resolve(__dirname, "./src"),
                "~": path.resolve(__dirname, "./src"),
            },
            extensions: ['.mjs', '.js', '.jsx', '.ts', '.tsx', '.json']
        },
        esbuild: {
            jsx: 'automatic'
        },
        server: {
            port: 1000, // Reverted back to 3000
            host: true,
            open: false,
            strictPort: true,
            proxy: {
                '/api': {
                    target: 'http://localhost:5202',
                    changeOrigin: true,
                    secure: false,
                    timeout: 10000,
                    configure: (proxy, _options) => {
                        proxy.on('error', (err, _req, _res) => {
                            console.log('Proxy error:', err);
                        });
                    }
                }
            }
        },
        build: {
            outDir: 'dist',
            sourcemap: true,
            rollupOptions: {
                output: {
                    manualChunks: {
                        vendor: ['react', 'react-dom'],
                        antd: ['antd'],
                        redux: ['@reduxjs/toolkit', 'react-redux'],
                        router: ['react-router', 'react-router-dom']
                    }
                }
            }
        },
        preview: {
            port: 4173,
            host: true,
            open: false
        }
    };
});