using System.ComponentModel.DataAnnotations;
using PortalTarefas.Web.Models;

namespace PortalTarefas.Web.Contratos;

// =====================================================================
// O CONTRATO DA API: o que entra e o que sai em JSON.
// A entidade Tarefa (o que está no banco) NUNCA sai direto pela API:
// cada operação tem o seu DTO, com só os campos que fazem sentido nela.
// =====================================================================

// ---------- ENTRADA ----------

// POST /api/tarefas — o cliente não escolhe Id, Situacao nem RowVersion
public record CriarTarefaDto(
    [Required(ErrorMessage = "Informe o título.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "O título deve ter entre 3 e 120 caracteres.")]
    string Titulo,

    [StringLength(500, ErrorMessage = "A descrição pode ter no máximo 500 caracteres.")]
    string? Descricao,

    Prioridade Prioridade,
    DateTime Prazo);

// PUT /api/tarefas/{id} — inclui a Situacao e a versão que o cliente leu
public record AtualizarTarefaDto(
    [Required(ErrorMessage = "Informe o título.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "O título deve ter entre 3 e 120 caracteres.")]
    string Titulo,

    [StringLength(500, ErrorMessage = "A descrição pode ter no máximo 500 caracteres.")]
    string? Descricao,

    Situacao Situacao,
    Prioridade Prioridade,
    DateTime Prazo,

    [Required(ErrorMessage = "Informe a versão (rowVersion) lida no GET.")]
    string RowVersion);

// ---------- SAÍDA ----------

// Item da LISTA: o mínimo para mostrar uma linha
public record TarefaResumoDto(
    int Id, string Titulo, Situacao Situacao, Prioridade Prioridade, DateTime Prazo);

// DETALHES: tudo o que o cliente precisa para exibir e editar
public record TarefaDetalhesDto(
    int Id, string Titulo, string? Descricao, Situacao Situacao,
    Prioridade Prioridade, DateTime Prazo, string RowVersion,
    string Equipe);                                              // Roteiro 6

// Roteiro 6: quem está logado — GET /api/conta/eu
public record UsuarioDto(
    string Email, IReadOnlyList<string> Papeis, string? Equipe, bool PodeEditarOutrasEquipes);

// Uma PÁGINA de resultados + os dados para navegar entre páginas
public record PaginaDto<T>(
    IReadOnlyList<T> Itens, int Pagina, int Tamanho, int TotalItens, int TotalPaginas);

// ---------- PARÂMETROS DE CONSULTA ----------

// Enums = LISTA PERMITIDA: só estes valores são aceitos (outro valor → 400)
public enum OrdenarPor { Prazo, Titulo, Prioridade }
public enum Direcao { Asc, Desc }

// O pacote de parâmetros que o endpoint entrega ao serviço
public record ConsultaTarefas(
    int Pagina, int Tamanho, OrdenarPor OrdenarPor, Direcao Direcao,
    Situacao? Situacao, Prioridade? Prioridade);

// ---------- CONVERSÕES entidade → DTO ----------
public static class TarefaMapeamento
{
    public static TarefaDetalhesDto ParaDetalhes(this Tarefa t) =>
    new(t.Id, t.Titulo, t.Descricao, t.Situacao, t.Prioridade, t.Prazo, t.RowVersion, t.Equipe);
}
