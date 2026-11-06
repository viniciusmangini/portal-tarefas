using PortalTarefas.Web.Contratos;
using PortalTarefas.Web.Models;

namespace PortalTarefas.Web.Services;

// O CONTRATO do serviço (Roteiro 2), usado pelas páginas MVC e pela API.
public interface ITarefaService
{
    Task<List<Tarefa>> ListarAsync(Situacao? situacao = null);
    Task<Tarefa?> ObterAsync(int id);
    Task<Tarefa> CriarAsync(Tarefa tarefa);
    Task<bool> AtualizarAsync(Tarefa tarefa);

    // Novos no Roteiro 4
    Task<PaginaDto<TarefaResumoDto>> ListarPaginaAsync(ConsultaTarefas consulta);
    Task<bool> RemoverAsync(int id);
}
