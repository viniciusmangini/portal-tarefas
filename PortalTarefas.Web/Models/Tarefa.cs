namespace PortalTarefas.Web.Models;

public enum Situacao { Pendente, EmAndamento, Concluida }

public enum Prioridade { Baixa, Media, Alta }

// Agora é uma ENTIDADE: cada objeto Tarefa corresponde a uma linha
// da tabela Tarefas no banco. As regras de coluna (tamanho, obrigatório,
// índice) ficam no PortalTarefasDbContext.
public class Tarefa
{
    public int Id { get; set; }                     // chave primária (por convenção, "Id")
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public Situacao Situacao { get; set; } = Situacao.Pendente;
    public Prioridade Prioridade { get; set; } = Prioridade.Media;
    public DateTime Prazo { get; set; } = DateTime.Today.AddDays(7);

    // Novo no Roteiro 3 — TOKEN DE CONCORRÊNCIA.
    // O SQLite não tem o "rowversion" automático do SQL Server, então
    // nós mesmos geramos um valor novo a cada alteração (ver TarefaServiceEf).
    // Roteiro 6: a equipe dona da tarefa ("Alfa", "Beta"...). Usada na política de edição.
    public string Equipe { get; set; } = string.Empty;
    public string RowVersion { get; set; } = Guid.NewGuid().ToString();
}
