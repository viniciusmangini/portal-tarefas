// Chamadas REAIS à API usando o cliente gerado.
// Cada chamada devolve { data, error, response }:
//   data     → corpo de sucesso (2xx), já tipado
//   error    → corpo de erro (Problem Details), já tipado
//   response → a resposta HTTP crua (status, cabeçalhos)
import { api } from "./api/cliente";

// 1) Lista paginada — tente trocar "Prazo" por "Senha": o TypeScript recusa.
const lista = await api.GET("/api/tarefas", {
  params: { query: { pagina: 1, tamanho: 5, ordenarPor: "Prazo", direcao: "Asc" } },
});
console.log(`GET /api/tarefas → ${lista.response.status} (${lista.data?.totalItens} tarefas)`);
for (const t of lista.data?.itens ?? []) {
  console.log(`   #${t.id} ${t.titulo} [${t.situacao}, ${t.prioridade}]`);
}

// 2) Cadastro — o corpo é checado contra o CriarTarefaDto do contrato
const criada = await api.POST("/api/tarefas", {
  body: { titulo: "Criada pelo cliente TypeScript", descricao: null, prioridade: "Media", prazo: "2026-12-01" },
});
console.log(`POST /api/tarefas → ${criada.response.status}, Location: ${criada.response.headers.get("Location")}`);

// 3) Recurso inexistente — o erro chega como Problem Details
const inexistente = await api.GET("/api/tarefas/{id}", { params: { path: { id: 999 } } });
console.log(`GET /api/tarefas/999 → ${inexistente.response.status}: ${inexistente.error?.title}`);