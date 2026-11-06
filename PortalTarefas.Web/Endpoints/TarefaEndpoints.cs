using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using PortalTarefas.Web.Contratos;
using PortalTarefas.Web.Models;
using PortalTarefas.Web.Services;

namespace PortalTarefas.Web.Endpoints;

public static class TarefaEndpoints
{
    public static void MapTarefaEndpoints(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/tarefas").WithTags("Tarefas");

        grupo.MapGet("/", async (
            ITarefaService servico,
            [Range(1, int.MaxValue, ErrorMessage = "A página começa em 1.")] int pagina = 1,
            [Range(1, 50, ErrorMessage = "O tamanho da página vai de 1 a 50.")] int tamanho = 10,
            OrdenarPor ordenarPor = OrdenarPor.Prazo,
            Direcao direcao = Direcao.Asc,
            Situacao? situacao = null,
            Prioridade? prioridade = null) =>
        {
            var consulta = new ConsultaTarefas(pagina, tamanho, ordenarPor, direcao, situacao, prioridade);
            return Results.Ok(await servico.ListarPaginaAsync(consulta));
        })
        .WithName("ListarTarefas")
        .WithSummary("Lista tarefas paginadas, com filtros e ordenação")
        .Produces<PaginaDto<TarefaResumoDto>>(StatusCodes.Status200OK)
        .ProducesValidationProblem();

        grupo.MapGet("/{id:int}", async (int id, ITarefaService servico) =>
            await servico.ObterAsync(id) is Tarefa tarefa
                ? Results.Ok(tarefa.ParaDetalhes())
                : Results.Problem(statusCode: 404, title: "Tarefa não encontrada",
                                  detail: $"Não existe tarefa com id {id}."))
        .WithName("ObterTarefa")
        .WithSummary("Obtém os detalhes de uma tarefa")
        .Produces<TarefaDetalhesDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        grupo.MapPost("/", async (CriarTarefaDto dto, ITarefaService servico) =>
        {
            var criada = await servico.CriarAsync(new Tarefa
            {
                Titulo = dto.Titulo,
                Descricao = dto.Descricao,
                Prioridade = dto.Prioridade,
                Prazo = dto.Prazo
            });

            return Results.CreatedAtRoute("ObterTarefa", new { id = criada.Id }, criada.ParaDetalhes());
        })
        .WithName("CriarTarefa")
        .WithSummary("Cadastra uma tarefa (nasce com situação Pendente)")
        .Produces<TarefaDetalhesDto>(StatusCodes.Status201Created)
        .ProducesValidationProblem();

        grupo.MapPut("/{id:int}", async (int id, AtualizarTarefaDto dto, ITarefaService servico) =>
        {
            try
            {
                var atualizou = await servico.AtualizarAsync(new Tarefa
                {
                    Id = id,
                    Titulo = dto.Titulo,
                    Descricao = dto.Descricao,
                    Situacao = dto.Situacao,
                    Prioridade = dto.Prioridade,
                    Prazo = dto.Prazo,
                    RowVersion = dto.RowVersion
                });

                return atualizou
                    ? Results.NoContent()
                    : Results.Problem(statusCode: 404, title: "Tarefa não encontrada",
                                      detail: $"Não existe tarefa com id {id}.");
            }
            catch (DbUpdateConcurrencyException)
            {
                return Results.Problem(statusCode: 409, title: "Conflito de edição",
                    detail: "A tarefa foi alterada por outra pessoa. Faça um novo GET e reaplique a alteração.");
            }
        })
        .WithName("AtualizarTarefa")
        .WithSummary("Atualiza uma tarefa (exige o rowVersion lido no GET)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        grupo.MapDelete("/{id:int}", async (int id, ITarefaService servico) =>
            await servico.RemoverAsync(id)
                ? Results.NoContent()
                : Results.Problem(statusCode: 404, title: "Tarefa não encontrada",
                                  detail: $"Não existe tarefa com id {id}."))
        .WithName("RemoverTarefa")
        .WithSummary("Remove uma tarefa")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}