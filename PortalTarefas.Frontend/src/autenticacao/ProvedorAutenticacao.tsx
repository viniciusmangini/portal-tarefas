import { useEffect, useState, type ReactNode } from "react";
import { api, chamar, type Usuario } from "../api/cliente";
import { ContextoAutenticacao } from "./contexto";

// A SPA NUNCA vê o cookie (HttpOnly): só pergunta à API "quem sou eu?" (GET /api/conta/eu).
export default function ProvedorAutenticacao({ children }: { children: ReactNode }) {
  const [usuario, setUsuario] = useState<Usuario | null>(null);
  const [verificando, setVerificando] = useState(true);

  async function carregarUsuario() {
    const resultado = await chamar(api.GET("/api/conta/eu"));
    setUsuario(resultado.ok ? resultado.dados : null);
  }

  // RESTAURAR SESSÃO: ao abrir o site (ou dar F5), se o cookie ainda vale, já estamos logados
  useEffect(() => {
    chamar(api.GET("/api/conta/eu")).then((resultado) => {
      setUsuario(resultado.ok ? resultado.dados : null); // 200 → logado; 401 → ninguém
      setVerificando(false);
    });
  }, []);

  async function entrar(email: string, senha: string) {
    // useCookies=true → a API responde com um cookie de sessão (o navegador guarda sozinho)
    const resultado = await chamar(
      api.POST("/api/conta/login", { params: { query: { useCookies: true } }, body: { email, password: senha } }),
    );
    if (!resultado.ok) {
      return resultado.erro.detalhe === "LockedOut"
        ? "Conta bloqueada temporariamente por excesso de tentativas."
        : "E-mail ou senha inválidos.";
    }
    await carregarUsuario();
    return null;
  }

  async function sair() {
    await chamar(api.POST("/api/conta/logout"));
    setUsuario(null);
  }

  const papeis = usuario?.papeis ?? [];
  const valor = {
    usuario,
    verificando,
    entrar,
    sair,
    podeCriar: papeis.includes("Usuario") || papeis.includes("Administrador"),
    ehAdministrador: papeis.includes("Administrador"),
  };

  return <ContextoAutenticacao.Provider value={valor}>{children}</ContextoAutenticacao.Provider>;
}