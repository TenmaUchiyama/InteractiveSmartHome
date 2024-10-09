import { defineConfig } from "vite";
import { svelte } from "@sveltejs/vite-plugin-svelte";
import path from "path";
// https://vitejs.dev/config/
export default defineConfig({
  plugins: [svelte()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
      "@nodes": path.resolve(__dirname, "./src/nodes"),
      "@type": path.resolve(__dirname, "./src/type"),
      "@utils": path.resolve(__dirname, "./src/utils"),
    },
  },
});
