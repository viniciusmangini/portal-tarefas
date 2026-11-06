import { NavLink, Outlet } from "react-router";
import { useAutenticacao } from "./autenticacao/contexto";

export default function App() {
  const { usuario, sair, podeCriar } = useAutenticacao();

  return (
    <>
      <header>
        <strong>Portal de Tarefas</strong>
        {usuario && <NavLink to="/tarefas" end>Tarefas</NavLink>}
        {/* Só ESCONDE o link (experiência). Quem bloqueia de verdade é a API (403). */}
        {podeCriar && <NavLink to="/tarefas/nova">Nova tarefa</NavLink>}
        <span className="sessao">
          {usuario ? (
            <>
              {usuario.email} ({usuario.papeis.join(", ")} · equipe {usuario.equipe})
              <button onClick={sair}>Sair</button>
            </>
          ) : (
            <NavLink to="/login">Entrar</NavLink>
          )}
        </span>
      </header>
      <main>
        <Outlet />
      </main>
    </>
  );
}