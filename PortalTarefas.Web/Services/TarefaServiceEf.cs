using Microsoft.EntityFrameworkCore;
using PortalTarefas.Web.Contratos;
using PortalTarefas.Web.Data;
using PortalTarefas.Web.Models;
using System.Diagnostics;
using PortalTarefas.Web.Observabilidade;

namespace PortalTarefas.Web.Services;

// Mesmo contrato do Roteiro 2 (ITarefaService), agora gravando no banco.
// Quem usa o serviço (endpoints, controller) não percebe a troca.
public class TarefaServiceEf(PortalTarefasDbContext db, ILogger<TarefaServiceEf> logger) : ITarefaService
{
    // AsNoTracking: só leitura — o EF não precisa "vigiar" esses objetos
    public Task<List<Tarefa>> ListarAsync(Situacao? situacao = null) =>
        db.Tarefas.AsNoTracking()
            .Where(t => situacao == null || t.Situacao == situacao)
            .OrderBy(t => t.Prazo)
            .ToListAsync();

    public Task<Tarefa?> ObterAsync(int id) =>
        db.Tarefas.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);

    public async Task<Tarefa> CriarAsync(Tarefa tarefa)
    {
        db.Tarefas.Add(tarefa);
        await db.SaveChangesAsync();

        // Roteiro 7 — MÉTRICA DE NEGÓCIO: +1 tarefa criada, separada por prioridade
        Telemetria.TarefasCriadas.Add(1, new KeyValuePair<string, object?>("prioridade", tarefa.Prioridade.ToString()));
        return tarefa;
    }

    public async Task<bool> AtualizarAsync(Tarefa dados)
    {
        // Roteiro 7 — ESCOPO: toda linha de log dentro deste bloco leva "TarefaId"
        using var escopo = logger.BeginScope("TarefaId:{TarefaId}", dados.Id);
        var cronometro = Stopwatch.StartNew();
        logger.LogInformation("Atualização de tarefa iniciada");

        var tarefa = await db.Tarefas.FindAsync(dados.Id);
        if (tarefa is null)
        {
            logger.LogInformation("Atualização de tarefa: não encontrada em {Ms} ms", cronometro.ElapsedMilliseconds);
            return false;
        }

        // CONCORRÊNCIA — passo 1: dizer ao EF qual versão o usuário tinha na tela.
        db.Entry(tarefa).Property(t => t.RowVersion).OriginalValue = dados.RowVersion;

        tarefa.Titulo = dados.Titulo;
        tarefa.Descricao = dados.Descricao;
        tarefa.Situacao = dados.Situacao;
        tarefa.Prioridade = dados.Prioridade;
        tarefa.Prazo = dados.Prazo;

        // CONCORRÊNCIA — passo 2: gerar a nova versão (o SQLite não faz isso sozinho).
        tarefa.RowVersion = Guid.NewGuid().ToString();

        // AUDITORIA: registrada junto com a alteração.
        db.EventosAuditoria.Add(new EventoAuditoria
        {
            TarefaId = tarefa.Id,
            Acao = "Atualizacao",
            DataHora = DateTime.Now
        });

        // TRANSAÇÃO: um único SaveChanges grava o UPDATE e o INSERT juntos.
        await db.SaveChangesAsync();
        logger.LogInformation("Atualização de tarefa concluída em {Ms} ms", cronometro.ElapsedMilliseconds);
        return true;
    }

    // ===================== Roteiro 4 =====================

    public async Task<PaginaDto<TarefaResumoDto>> ListarPaginaAsync(ConsultaTarefas c)
    {
        // Roteiro 7 — SPAN CUSTOMIZADO: um trecho com nome próprio dentro do trace da requisição
        using var atividade = Telemetria.Fonte.StartActivity("ListarPagina");
        atividade?.SetTag("tarefas.pagina", c.Pagina);
        var cronometro = Stopwatch.StartNew();

        var consulta = db.Tarefas.AsNoTracking();

        if (c.Situacao is not null) consulta = consulta.Where(t => t.Situacao == c.Situacao);
        if (c.Prioridade is not null) consulta = consulta.Where(t => t.Prioridade == c.Prioridade);

        var desc = c.Direcao == Direcao.Desc;
        var ordenada = c.OrdenarPor switch
        {
            OrdenarPor.Titulo => desc ? consulta.OrderByDescending(t => t.Titulo) : consulta.OrderBy(t => t.Titulo),
            OrdenarPor.Prioridade => desc ? consulta.OrderByDescending(t => t.Prioridade) : consulta.OrderBy(t => t.Prioridade),
            _ => desc ? consulta.OrderByDescending(t => t.Prazo) : consulta.OrderBy(t => t.Prazo),
        };

        var total = await consulta.CountAsync();

        var itens = await ordenada
            .ThenBy(t => t.Id)
            .Skip((c.Pagina - 1) * c.Tamanho)
            .Take(c.Tamanho)
            .Select(t => new TarefaResumoDto(t.Id, t.Titulo, t.Situacao, t.Prioridade, t.Prazo))
            .ToListAsync();

        var totalPaginas = (int)Math.Ceiling(total / (double)c.Tamanho);

        // Roteiro 7 — MÉTRICA TÉCNICA: duração da listagem, separada pela ordenação usada
        Telemetria.DuracaoListagem.Record(cronometro.Elapsed.TotalMilliseconds,
            new KeyValuePair<string, object?>("ordenarPor", c.OrdenarPor.ToString()));
        atividade?.SetTag("tarefas.total", total);
        return new PaginaDto<TarefaResumoDto>(itens, c.Pagina, c.Tamanho, total, totalPaginas);
    }

    public async Task<bool> RemoverAsync(int id)
    {
        var tarefa = await db.Tarefas.FindAsync(id);
        if (tarefa is null) return false;

        db.Tarefas.Remove(tarefa);
        await db.SaveChangesAsync();
        return true;
    }
}
