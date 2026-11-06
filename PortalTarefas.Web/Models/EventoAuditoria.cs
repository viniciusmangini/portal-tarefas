namespace PortalTarefas.Web.Models;

// Um registro de "quem mexeu em quê": gravado JUNTO com a alteração
// da tarefa, na mesma transação.
public class EventoAuditoria
{
    public int Id { get; set; }
    public int TarefaId { get; set; }
    public string Acao { get; set; } = string.Empty;
    public DateTime DataHora { get; set; }
}
