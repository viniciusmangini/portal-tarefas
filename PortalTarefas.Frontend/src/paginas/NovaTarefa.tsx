import { useState } from "react";
import { useNavigate } from "react-router";
import { api, chamar, type ErroApi, type Prioridade } from "../api/cliente";
import MensagemErro from "../componentes/MensagemErro";

export default function NovaTarefa() {
  const navegar = useNavigate();
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState<ErroApi | null>(null);

  async function salvar(evento: React.FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const dados = new FormData(evento.currentTarget);
    setEnviando(true);
    setErro(null);

    const resultado = await chamar(
      api.POST("/api/tarefas", {
        body: {
          titulo: String(dados.get("titulo")),
          descricao: String(dados.get("descricao")) || null,
          prioridade: dados.get("prioridade") as Prioridade,
          prazo: String(dados.get("prazo")),
        },
      }),
    );

    setEnviando(false); // sempre sai do "enviando", com sucesso ou erro
    if (resultado.ok) navegar(`/tarefas/${resultado.dados.id}`); // vai para os detalhes da nova tarefa
    else setErro(resultado.erro);
  }

  // Erros de validação devolvidos pela API, por campo (as chaves vêm do DTO: "Titulo", "Descricao"...)
  const erroDoCampo = (campo: string) => erro?.campos?.[campo]?.join(" ");

  return (
    <section>
      <h1>Nova tarefa</h1>

      {erro && !erro.campos && <MensagemErro erro={erro} />}

      <form className="formulario" onSubmit={salvar}>
        <label>
          Título
          <input name="titulo" />
        </label>
        {erroDoCampo("Titulo") && <span className="erro-campo">{erroDoCampo("Titulo")}</span>}

        <label>
          Descrição
          <textarea name="descricao" />
        </label>
        {erroDoCampo("Descricao") && <span className="erro-campo">{erroDoCampo("Descricao")}</span>}

        <label>
          Prioridade
          <select name="prioridade" defaultValue="Media">
            <option value="Baixa">Baixa</option>
            <option value="Media">Média</option>
            <option value="Alta">Alta</option>
          </select>
        </label>

        <label>
          Prazo
          <input name="prazo" type="date" required />
        </label>

        <button type="submit" disabled={enviando}>{enviando ? "Salvando…" : "Salvar"}</button>
      </form>
    </section>
  );
}