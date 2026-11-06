import { useState } from "react";
import { Navigate, useLocation, useNavigate } from "react-router";
import { useAutenticacao } from "../autenticacao/contexto";

export default function Login() {
  const { usuario, entrar } = useAutenticacao();
  const navegar = useNavigate();
  const local = useLocation();
  const [erro, setErro] = useState<string | null>(null);
  const [enviando, setEnviando] = useState(false);

  // Para onde voltar depois do login (a URL que o usuário tentou abrir)
  const destino = (local.state as { de?: string } | null)?.de ?? "/tarefas";

  if (usuario) return <Navigate to={destino} replace />; // já logado

  async function enviar(evento: React.FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    const dados = new FormData(evento.currentTarget);
    setEnviando(true);
    const mensagem = await entrar(String(dados.get("email")), String(dados.get("senha")));
    setEnviando(false);
    if (mensagem) setErro(mensagem);
    else navegar(destino, { replace: true });
  }

  return (
    <section>
      <h1>Entrar</h1>
      {erro && <div className="erro" role="alert">{erro}</div>}
      <form className="formulario" onSubmit={enviar}>
        <label>
          E-mail
          <input name="email" type="email" autoComplete="username" required />
        </label>
        <label>
          Senha
          <input name="senha" type="password" autoComplete="current-password" required />
        </label>
        <button type="submit" disabled={enviando}>{enviando ? "Entrando…" : "Entrar"}</button>
      </form>
    </section>
  );
}