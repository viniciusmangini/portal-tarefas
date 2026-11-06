import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router";
import { api, chamar, type Direcao, type ErroApi, type OrdenarPor, type Pagina, type Resultado, type Situacao } from "../api/cliente";
import MensagemErro from "../componentes/MensagemErro";

// Os CINCO estados de uma requisição — a tela mostra exatamente um deles por vez
type Estado =
  | { fase: "ocioso" }                       // ainda não buscou
  | { fase: "carregando" }
  | { fase: "vazio" }                        // buscou e não veio nada
  | { fase: "sucesso"; pagina: Pagina }
  | { fase: "erro"; erro: ErroApi };

export default function ListaTarefas() {
  // Filtros, ordenação e página moram na URL: /tarefas?situacao=Pendente&pagina=2...
  // Assim, copiar o link (ou dar F5) reabre a MESMA lista.
  const [params, setParams] = useSearchParams();
  const [tentativa, setTentativa] = useState(0);

  // A "chave" identifica UMA busca: muda quando a URL muda ou quando o usuário tenta de novo
  const chave = `${params.toString()}#${tentativa}`;

  // Guardamos a última resposta JUNTO com a chave da busca que a produziu
  const [resposta, setResposta] = useState<{ chave: string; resultado: Resultado<Pagina> } | null>(null);

  useEffect(() => {
    if (params.size === 0) return; // sem filtros na URL: nada a buscar

    const controle = new AbortController();

    chamar(
      api.GET("/api/tarefas", {
        params: {
          query: {
            pagina: Number(params.get("pagina") ?? 1),
            tamanho: Number(params.get("tamanho") ?? 5),
            ordenarPor: (params.get("ordenarPor") ?? "Prazo") as OrdenarPor,
            direcao: (params.get("direcao") ?? "Asc") as Direcao,
            situacao: (params.get("situacao") || undefined) as Situacao | undefined,
          },
        },
        signal: controle.signal, // permite cancelar a requisição
      }),
    ).then((resultado) => {
      if (controle.signal.aborted) return; // resposta OBSOLETA: a URL já mudou, ignora
      setResposta({ chave, resultado });
    });

    // Limpeza: roda quando a URL muda ou o usuário sai da tela → cancela a requisição em andamento
    return () => controle.abort();
  }, [chave, params]);

  // O estado da tela é CALCULADO (não guardado): sempre exatamente um dos cinco
  const estado: Estado =
    params.size === 0 ? { fase: "ocioso" }
    : !resposta || resposta.chave !== chave ? { fase: "carregando" } // a resposta desta busca ainda não chegou
    : !resposta.resultado.ok ? { fase: "erro", erro: resposta.resultado.erro }
    : resposta.resultado.dados.itens.length === 0 ? { fase: "vazio" }
    : { fase: "sucesso", pagina: resposta.resultado.dados };

  // Formulário de filtros → grava na URL (sempre voltando para a página 1)
  function buscar(evento: React.FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const dados = new FormData(evento.currentTarget);
    const novos = new URLSearchParams({ pagina: "1" });
    for (const [chave, valor] of dados) if (valor) novos.set(chave, String(valor));
    setParams(novos);
  }

  function irParaPagina(numero: number) {
    const novos = new URLSearchParams(params);
    novos.set("pagina", String(numero));
    setParams(novos);
  }

  return (
    <section>
      <h1>Tarefas</h1>

      {/* key: recria o formulário quando a URL muda, para os campos refletirem a URL */}
      <form className="filtros" onSubmit={buscar} key={params.toString()}>
        <select name="situacao" defaultValue={params.get("situacao") ?? ""}>
          <option value="">Todas as situações</option>
          <option value="Pendente">Pendente</option>
          <option value="EmAndamento">Em andamento</option>
          <option value="Concluida">Concluída</option>
        </select>
        <select name="ordenarPor" defaultValue={params.get("ordenarPor") ?? "Prazo"}>
          <option value="Prazo">Prazo</option>
          <option value="Titulo">Título</option>
          <option value="Prioridade">Prioridade</option>
        </select>
        <select name="direcao" defaultValue={params.get("direcao") ?? "Asc"}>
          <option value="Asc">Crescente</option>
          <option value="Desc">Decrescente</option>
        </select>
        <button type="submit">Buscar</button>
      </form>

      {estado.fase === "ocioso" && <p>Escolha os filtros e clique em <strong>Buscar</strong>.</p>}

      {estado.fase === "carregando" && <p role="status">Carregando tarefas…</p>}

      {estado.fase === "vazio" && <p>Nenhuma tarefa encontrada para estes filtros.</p>}

      {estado.fase === "erro" && (
        <MensagemErro erro={estado.erro} aoTentarDeNovo={() => setTentativa((t) => t + 1)} />
      )}

      {estado.fase === "sucesso" && (
        <>
          <table>
            <thead>
              <tr><th>Título</th><th>Situação</th><th>Prioridade</th><th>Prazo</th></tr>
            </thead>
            <tbody>
              {estado.pagina.itens.map((t) => (
                <tr key={t.id}>
                  <td><Link to={`/tarefas/${t.id}`}>{t.titulo}</Link></td>
                  <td>{t.situacao}</td>
                  <td>{t.prioridade}</td>
                  <td>{new Date(t.prazo).toLocaleDateString("pt-BR")}</td>
                </tr>
              ))}
            </tbody>
          </table>

          <nav className="paginacao">
            <button disabled={estado.pagina.pagina <= 1} onClick={() => irParaPagina(estado.pagina.pagina - 1)}>
              ← Anterior
            </button>
            <span>
              Página {estado.pagina.pagina} de {estado.pagina.totalPaginas} ({estado.pagina.totalItens} tarefas)
            </span>
            <button
              disabled={estado.pagina.pagina >= estado.pagina.totalPaginas}
              onClick={() => irParaPagina(estado.pagina.pagina + 1)}
            >
              Próxima →
            </button>
          </nav>
        </>
      )}
    </section>
  );
}