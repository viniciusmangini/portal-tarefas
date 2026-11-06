
# Implantação — Portal de Tarefas

## Como identificar uma versão

- Cada imagem tem como **tag o commit** que a gerou: `ghcr.io/<usuario>/<repositorio>:<commit>`.
- A versão que está rodando aparece em `GET /api/diagnostico`, no campo `"versao"`.
- O artefato do pipeline também leva o commit no nome: `portaltarefas-<commit>`.

## Promoção (teste → produção)

1. O pipeline constrói a imagem **uma única vez** e a testa no ambiente de teste (job `implantar`: Health Check + smoke test).
2. Só uma imagem aprovada é publicada no registro (`docker push`).
3. Produção recebe **a mesma imagem, com a mesma tag**, nunca um build novo.
4. Depois de implantar: conferir `/health/ready` (200) e o campo `"versao"` de `/api/diagnostico`.

## Rollback (voltar à versão anterior)

1. Na tabela abaixo, pegar a tag da **última versão que funcionava**.
2. Implantar essa tag de novo: `docker run ... ghcr.io/<usuario>/<repositorio>:<commit-anterior>`.
3. Conferir `/health/ready` e o campo `"versao"` de `/api/diagnostico`.

> Voltar a imagem não desfaz mudanças no banco. Uma migration nova precisa ser compatível com a versão anterior, ou o rollback exige restaurar o banco.

## Registro das versões implantadas

| Data | Ambiente | Versão (commit) | Responsável | Resultado |
| ---- | -------- | ---------------- | ------------ | --------- |
|      | teste    |                  |              |           |
