namespace PortalTarefas.Web.Endpoints;

// Roteiro 7 — o SERVIÇO EXTERNO de que o Portal "depende" (Etapas 3 e 4).
// Para não precisar de outro sistema, a própria API finge ser esse serviço em /simulador.
public static class ExternoEndpoints
{
    // Modo atual: "ok" (sempre responde), "instavel" (falha 1 a cada 3) ou "fora" (sempre falha)
    private static string _modo = "ok";
    private static int _contador;

    public static void MapExternoEndpoints(this IEndpointRouteBuilder app)
    {
        var simulador = app.MapGroup("/simulador").ExcludeFromDescription();

        // GET /simulador/externo — o "serviço externo" propriamente dito
        simulador.MapGet("/externo", () =>
        {
            var falhar = _modo switch
            {
                "fora" => true,
                "instavel" => Interlocked.Increment(ref _contador) % 3 == 1,   // 1ª, 4ª, 7ª... chamadas falham
                _ => false
            };
            return falhar ? Results.StatusCode(503) : Results.Ok(new { status = "ok" });
        });

        // POST /simulador/modo/fora — muda o comportamento do serviço externo
        simulador.MapPost("/modo/{modo}", (string modo) =>
        {
            _modo = modo;
            _contador = 0;
            return Results.Ok(new { modo });
        });

        // GET /api/externo — o Portal CHAMANDO o serviço externo pelo HttpClient "externo"
        app.MapGet("/api/externo", async (IHttpClientFactory fabrica) =>
        {
            try
            {
                var resposta = await fabrica.CreateClient("externo").GetAsync("simulador/externo");
                return resposta.IsSuccessStatusCode
                    ? Results.Ok(new { servicoExterno = "disponível" })
                    : Results.Problem(statusCode: 503, title: "Serviço externo indisponível",
                                      detail: $"Respondeu {(int)resposta.StatusCode}.");
            }
            catch (Exception ex)
            {
                // O NOME da exceção mostra o motivo: BrokenCircuitException = circuito aberto;
                // TimeoutRejectedException = demorou demais. Nenhum dado sensível vai junto.
                return Results.Problem(statusCode: 503, title: "Serviço externo indisponível",
                                       detail: ex.GetType().Name);
            }
        }).ExcludeFromDescription();
    }
}