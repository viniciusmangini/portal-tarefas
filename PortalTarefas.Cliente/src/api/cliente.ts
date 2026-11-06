// O CLIENTE da API: uma linha de configuração.
// Todos os tipos (rotas, parâmetros, corpos, respostas) vêm de schema.d.ts,
// que é GERADO a partir do openapi.json — nada do contrato é escrito à mão aqui.
import createClient from "openapi-fetch";
import type { paths } from "./schema";

// A API precisa estar rodando com o perfil "http" (dotnet run --launch-profile http)
export const api = createClient<paths>({ baseUrl: "http://localhost:5267" });
