using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace PortalTarefas.Web.Observabilidade;

// Roteiro 7 — os "instrumentos de medição" da nossa aplicação, num lugar só.
// ActivitySource cria SPANS (trechos de um trace); Meter cria MÉTRICAS (números ao longo do tempo).
public static class Telemetria
{
    public const string Nome = "PortalTarefas";

    // Fonte dos nossos spans customizados
    public static readonly ActivitySource Fonte = new(Nome);

    private static readonly Meter Medidor = new(Nome);

    // Métrica de NEGÓCIO: quantas tarefas foram criadas (dimensão: prioridade → só 3 valores)
    public static readonly Counter<long> TarefasCriadas =
        Medidor.CreateCounter<long>("portaltarefas.tarefas.criadas", description: "Tarefas criadas");

    // Métrica TÉCNICA: quanto tempo a listagem paginada leva (dimensão: ordenarPor → só 3 valores)
    public static readonly Histogram<double> DuracaoListagem =
        Medidor.CreateHistogram<double>("portaltarefas.listagem.duracao", unit: "ms",
            description: "Duração da listagem paginada");
}