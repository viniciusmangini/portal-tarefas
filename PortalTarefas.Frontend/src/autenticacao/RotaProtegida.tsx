import { Navigate, Outlet, useLocation } from "react-router";
import { useAutenticacao } from "./contexto";

// Sem sessão → vai para /login, levando o endereço pretendido (para voltar depois)
export default function RotaProtegida() {
  const { usuario, verificando } = useAutenticacao();
  const local = useLocation();

  if (verificando) return <p role="status">Verificando sessão…</p>;

  if (!usuario) {
    return <Navigate to="/login" replace state={{ de: local.pathname + local.search }} />;
  }

  return <Outlet />;
}