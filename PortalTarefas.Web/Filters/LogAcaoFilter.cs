using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PortalTarefas.Web.Filters;

// Filtro de AÇÃO: roda em volta de cada ação de controller,
// DEPOIS do model binding e da validação (o ModelState já está pronto).
public class LogAcaoFilter : IAsyncActionFilter
{
    private readonly ILogger<LogAcaoFilter> _logger;

    public LogAcaoFilter(ILogger<LogAcaoFilter> logger) => _logger = logger;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var cronometro = Stopwatch.StartNew();

        var executado = await next();   // executa a ação do controller

        cronometro.Stop();
        _logger.LogInformation(
            "Ação {Acao} | {Ms} ms | resultado: {Resultado} | modelo válido: {Valido}",
            context.ActionDescriptor.DisplayName,
            cronometro.ElapsedMilliseconds,
            executado.Result?.GetType().Name ?? "nenhum",
            context.ModelState.IsValid);
    }
}
