import type { ErroApi } from "../api/cliente";

// Mostra qualquer ErroApi do mesmo jeito, com botão opcional de nova tentativa
export default function MensagemErro({ erro, aoTentarDeNovo }: { erro: ErroApi; aoTentarDeNovo?: () => void }) {
  return (
    <div className="erro" role="alert">
      <strong>{erro.titulo}</strong>
      {erro.detalhe && <p>{erro.detalhe}</p>}
      {erro.status && <small>Status HTTP {erro.status}</small>}
      {aoTentarDeNovo && <button onClick={aoTentarDeNovo}>Tentar de novo</button>}
    </div>
  );
}