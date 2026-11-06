import react from '@vitejs/plugin-react'
import { defineConfig, loadEnv } from 'vite'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  // Lê o .env.development (o '' carrega também variáveis sem o prefixo VITE_)
  const env = loadEnv(mode, process.cwd(), '')

  return {
    plugins: [react()],
    server: {
      port: 5173,
      // Se a 5173 estiver ocupada (outro "npm run dev" esquecido aberto), PARA com erro
      // em vez de subir em outra porta (5174...) sem avisar.
      strictPort: true,
      // PROXY DE DESENVOLVIMENTO: o navegador pede /api/... ao próprio Vite (porta 5173),
      // e o Vite repassa para a API (porta 5267). Para o navegador é tudo a MESMA origem,
      // então não há CORS envolvido.
      proxy: { '/api': env.API_ALVO },
    },
  }
})