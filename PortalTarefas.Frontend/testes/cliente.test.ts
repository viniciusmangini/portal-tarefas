// Roteiro 8 — testes do front-end, rodados pelo PIPELINE a cada push (npm test).
// Usa o executor de testes que já vem no Node (node:test): nenhum pacote a instalar.
// Testa a função chamar() do Roteiro 5, que transforma qualquer resposta num Resultado.
import { test } from "node:test";
import assert from "node:assert/strict";
import { chamar } from "../src/api/cliente.ts";

test("resposta 2xx vira { ok: true } com os dados", async () => {
  const resultado = await chamar(Promise.resolve({
    data: { id: 1 },
    response: new Response(null, { status: 200 }),
  }));

  assert.deepEqual(resultado, { ok: true, dados: { id: 1 } });
});

test("Problem Details vira erro do tipo 'problema' com o título da API", async () => {
  const resultado = await chamar(Promise.resolve({
    error: { title: "Tarefa não encontrada" },
    response: new Response(null, { status: 404, headers: { "content-type": "application/problem+json" } }),
  }));

  assert.equal(resultado.ok, false);
  if (!resultado.ok) {
    assert.equal(resultado.erro.tipo, "problema");
    assert.equal(resultado.erro.titulo, "Tarefa não encontrada");
  }
});

test("falha de rede vira erro do tipo 'rede'", async () => {
  const resultado = await chamar(Promise.reject(new TypeError("Failed to fetch")));

  assert.equal(resultado.ok, false);
  if (!resultado.ok) assert.equal(resultado.erro.tipo, "rede");
});