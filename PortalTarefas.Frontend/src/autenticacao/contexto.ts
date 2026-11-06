import { createContext, useContext } from "react";
import type { Usuario } from "../api/cliente";

export type Autenticacao = {
  usuario: Usuario | null;            // null = ninguém logado
  verificando: boolean;               // true enquanto restaura a sessão ao abrir o site
  entrar: (email: string, senha: string) => Promise<string | null>; // devolve mensagem de erro ou null
  sair: () => Promise<void>;
  podeCriar: boolean;
  ehAdministrador: boolean;
};

export const ContextoAutenticacao = createContext<Autenticacao | null>(null);

export function useAutenticacao() {
  const contexto = useContext(ContextoAutenticacao);
  if (!contexto) throw new Error("useAutenticacao precisa estar dentro de <ProvedorAutenticacao>");
  return contexto;
}