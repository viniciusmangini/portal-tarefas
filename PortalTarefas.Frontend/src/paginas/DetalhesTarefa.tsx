import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router";
import { api, chamar, type ErroApi, type Resultado, type TarefaDetalhes } from "../api/cliente";
import MensagemErro from "../componentes/MensagemErro";
import { useAutenticacao } from "../autenticacao/contexto";

type Estado =
  | { fase: "carregando" }
  | { fase: "sucesso"; tarefa: TarefaDetalhes }
  | { fase: "erro"; erro: ErroApi };

export default function DetalhesTarefa() {
  const { id } = useParams();
  const { ehAdministrador } = useAutenticacao();   // Roteiro 6
  const navegar = useNavigate();
  const [erroExclusao, setErroExclusao] = useState<ErroApi | null>(null);
  const [resposta, setResposta] = useState<{ id?: string; resultado: Resultado<TarefaDetalhes> } | null>(null);

  useEffect(() => {
    const controle = new AbortController();

    chamar(api.GET("/api/tarefas/{id}", { params: { path: { id: Number(id) } }, signal: controle.signal }))
      .then((resultado) => {
        if (!controle.signal.aborted) setResposta({ id, resultado });
      });

    return () => controle.abort();
  }, [id]);

  const estado: Estado =
    !resposta || resposta.id !== id ? { fase: "carregando" }
    : resposta.resultado.ok ? { fase: "sucesso", tarefa: resposta.resultado.dados }
    : { fase: "erro", erro: resposta.resultado.erro };

  if (estado.fase === "carregando") return <p role="status">Carregando…</p>;

  if (estado.fase === "erro") {
    return (
      <section>
        {estado.erro.status === 404 ? (
          <h1>Tarefa não encontrada</h1>
        ) : (
          <MensagemErro erro={estado.erro} />
        )}
        <Link to="/tarefas">Voltar para a lista</Link>
      </section>
    );
  }

  const t = estado.tarefa;

  // Roteiro 6: o botão só aparece para administradores; a API confere de novo
  async function excluir() {
    if (!confirm(`Excluir "${t.titulo}"?`)) return;
    const resultado = await chamar(api.DELETE("/api/tarefas/{id}", { params: { path: { id: t.id } } }));
    if (resultado.ok) navegar("/tarefas");
    else setErroExclusao(resultado.erro);
  }

  return (
    <section>
      <h1>{t.titulo}</h1>
      <dl>
        <dt>Descrição</dt><dd>{t.descricao ?? "Sem descrição."}</dd>
        <dt>Situação</dt><dd>{t.situacao}</dd>
        <dt>Prioridade</dt><dd>{t.prioridade}</dd>
        <dt>Prazo</dt><dd>{new Date(t.prazo).toLocaleDateString("pt-BR")}</dd>
        <dt>Equipe</dt><dd>{t.equipe}</dd>
      </dl>
      {erroExclusao && <MensagemErro erro={erroExclusao} />}
      {ehAdministrador && <button onClick={excluir}>Excluir</button>}{" "}
      <Link to="/tarefas">Voltar para a lista</Link>
    </section>
  );
}