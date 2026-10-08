import { fileURLToPath, URL } from 'node:url';

import { PrimeVueResolver } from '@primevue/auto-import-resolver';
import vue from '@vitejs/plugin-vue';
import Components from 'unplugin-vue-components/vite';
import { defineConfig, loadEnv } from 'vite';
import vueDevTools from 'vite-plugin-vue-devtools';

// https://vitejs.dev/config/
export default defineConfig(({ command, mode }) => {
    const env = loadEnv(mode, process.cwd(), '');
    const vite_env = env.VITE_ENV;

    return {
        optimizeDeps: {
            include: ['quill'],
            noDiscovery: true
        },
        plugins: [
            vue(),
            Components({
                resolvers: [PrimeVueResolver()]
            }),
            vueDevTools()
        ],
        resolve: {
            alias: {
                '@': fileURLToPath(new URL('./src', import.meta.url))
            }
        },
        // TODO: 舊後台撤換之前先與測試區一樣的路徑
        base: vite_env === 'development' ? '/' : vite_env === 'testing' ? '/ubot_cms_new' : '/ubot_cms_new'
    };
});
