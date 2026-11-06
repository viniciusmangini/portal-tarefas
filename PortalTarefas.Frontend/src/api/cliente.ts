// =====================================================================
// CAMADA DE ACESSO À API — o ÚNICO lugar do front-end que fala HTTP.
// As páginas chamam api.GET/POST(...) sempre embrulhado em chamar(...),
// e recebem de volta um Resultado: ou os dados, ou um erro já "traduzido".
// =====================================================================
import createClient from "openapi-fetch";
import type { components, paths } from "./schema"; // schema.d.ts é GERADO (npm run gerar)

// baseUrl vazia = mesma origem. O proxy do Vite repassa /api para a API.
export const api = createClient<paths>({ baseUrl: "" });

// ---------- Tipos vindos do contrato (nada escrito à mão) ----------
export type Pagina = components["schemas"]["PaginaDtoOfTarefaResumoDto"];
export type TarefaResumo = components["schemas"]["TarefaResumoDto"];
export type TarefaDetalhes = components["schemas"]["TarefaDetalhesDto"];
export type NovaTarefa = components["schemas"]["CriarTarefaDto"];
export type Situacao = components["schemas"]["Situacao"];
export type Prioridade = components["schemas"]["Prioridade"];
export type OrdenarPor = components["schemas"]["OrdenarPor"];
export type Direcao = components["schemas"]["Direcao"];
export type Usuario = components["schemas"]["UsuarioDto"];   // Roteiro 6


// ---------- Erro NORMALIZADO: o mesmo formato para qualquer falha ----------
export type ErroApi = {
  tipo: "rede" | "http" | "problema"; // sem conexão | status sem Problem Details | Problem Details
  status?: number;
  titulo: string;
  detalhe?: string;
  campos?: Record<string, string[]>;   // erros de validação por campo (400)
};

export type Resultado<T> = { ok: true; dados: T } | { ok: false; erro: ErroApi };

// A função central: recebe a chamada do openapi-fetch e devolve um Resultado.
export async function chamar<T>(
  requisicao: Promise<{ data?: T; error?: unknown; response: Response }>,
): Promise<Resultado<T>> {
  try {
    const { data, error, response } = await requisicao;

    // 1) Sucesso (2xx)
    if (response.ok) return { ok: true, dados: data as T };

    // 2) Erro com Problem Details (application/problem+json)
    const tipoConteudo = response.headers.get("content-type") ?? "";
    if (tipoConteudo.includes("problem+json") && error && typeof error === "object") {
      const problema = error as { title?: string | null; detail?: string | null; errors?: Record<string, string[]> };
      return {
        ok: false,
        erro: {
          tipo: "problema",
          status: response.status,
          titulo: problema.title ?? `Erro ${response.status}`,
          detalhe: problema.detail ?? undefined,
          campos: problema.errors,
        },
      };
    }

    // 3) Erro HTTP sem Problem Details (ex.: proxy sem a API no ar)
    return { ok: false, erro: { tipo: "http", status: response.status, titulo: `O servidor respondeu ${response.status}.` } };
  } catch {
    // 4) Falha de REDE: a requisição nem chegou a ter resposta
    //    (também cai aqui quando a chamada é cancelada — quem chamou ignora)
    return { ok: false, erro: { tipo: "rede", titulo: "Não foi possível falar com o servidor. Verifique a conexão." } };
  }
}