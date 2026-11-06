using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace PortalTarefas.Web.Observabilidade;

// Roteiro 7 — verificação de PRONTIDÃO da dependência externa:
// "o serviço externo de que dependemos está respondendo?"
public class ServicoExternoHealthCheck(IHttpClientFactory fabrica) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var cliente = fabrica.CreateClient("externo");
            var resposta = await cliente.GetAsync("simulador/externo", cancellationToken);
            return resposta.IsSuccessStatusCode
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy($"Serviço externo respondeu {(int)resposta.StatusCode}");
        }
        catch (Exception ex)
        {
            // O detalhe vai para o LOG do servidor, não para a resposta pública
            return HealthCheckResult.Unhealthy("Serviço externo inacessível", ex);
        }
    }
}