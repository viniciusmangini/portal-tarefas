using Dapper;
using Microsoft.EntityFrameworkCore;
using PortalTarefas.Web.Data;
using PortalTarefas.Web.Models;

namespace PortalTarefas.Web.Endpoints;

// Etapa 4: a MESMA consulta de painel escrita de duas formas.
// Painel = tarefas ainda não concluídas, da mais urgente para a menos.
public static class PainelEndpoints
{
    public static void MapPainelEndpoints(this IEndpointRouteBuilder app)
    {
        // Roteiro 7 — RATE LIMITING: o grupo inteiro usa a política "publico" (5 requisições a cada 10 s)
        var grupo = app.MapGroup("").ExcludeFromDescription()
            .RequireRateLimiting("publico");

        // Versão 1 — EF Core: escrevemos C# (LINQ), o EF gera o SQL.
        grupo.MapGet("/api/painel/ef", async (PortalTarefasDbContext db, ILogger<Program> logger) =>
        {
            logger.LogInformation("Painel calculado no banco");      // Roteiro 7: some quando vem do cache
            return await db.Tarefas
                .AsNoTracking()                                   // só leitura
                .Where(t => t.Situacao != Situacao.Concluida)
                .OrderBy(t => t.Prazo)
                .Select(t => new PainelItem                       // projeção: só 4 colunas
                {
                    Id = t.Id, Titulo = t.Titulo, Prioridade = t.Prioridade, Prazo = t.Prazo
                })
                .ToListAsync();
        })
        // Roteiro 7 — OUTPUT CACHING: a resposta fica guardada por 20 s; nesse tempo o banco nem é consultado
        .CacheOutput(politica => politica.Expire(TimeSpan.FromSeconds(20)));

        // Versão 2 — Dapper: escrevemos o SQL, o Dapper só preenche os objetos.
        grupo.MapGet("/api/painel/dapper", async (PortalTarefasDbContext db) =>
        {
            const string sql = """
                SELECT Id, Titulo, Prioridade, Prazo
                FROM Tarefas
                WHERE Situacao <> @concluida
                ORDER BY Prazo
                """;

            // @concluida é um PARÂMETRO: o valor viaja separado do texto do SQL.
            // NUNCA montar o SQL com $"... {valor}" — isso abre brecha para SQL Injection.
            var conexao = db.Database.GetDbConnection();
            return await conexao.QueryAsync<PainelItem>(sql, new { concluida = (int)Situacao.Concluida });
        });

        // Evidência da auditoria
        grupo.MapGet("/api/auditoria", async (PortalTarefasDbContext db) =>
            await db.EventosAuditoria.AsNoTracking().OrderByDescending(e => e.Id).ToListAsync());
    }
}