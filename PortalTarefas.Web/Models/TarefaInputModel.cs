using System.ComponentModel.DataAnnotations;

namespace PortalTarefas.Web.Models;

// O que o FORMULÁRIO envia. Separado de Tarefa para validar só
// o que o usuário digita (ele não escolhe o Id, por exemplo).
public class TarefaInputModel : IValidatableObject
{
    [Display(Name = "Título")]
    [Required(ErrorMessage = "Informe o título.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "O título deve ter entre 3 e 120 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Display(Name = "Situação")]
    public Situacao Situacao { get; set; } = Situacao.Pendente;
    
    [Display(Name = "Descrição")]
    [StringLength(500, ErrorMessage = "A descrição pode ter no máximo 500 caracteres.")]
    public string? Descricao { get; set; }

    [Display(Name = "Prioridade")]
    public Prioridade Prioridade { get; set; } = Prioridade.Media;

    [Display(Name = "Prazo")]
    [DataType(DataType.Date)]
    public DateTime Prazo { get; set; } = DateTime.Today.AddDays(7);

    // Roteiro 3: a versão da tarefa quando o formulário foi aberto.
    // Viaja num campo oculto e volta no POST para detectar edição simultânea.
    public string? RowVersion { get; set; }

    // Validação customizada: regra que envolve MAIS DE UM campo
    // (prioridade + prazo). Roda depois das anotações acima.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Prazo.Date < DateTime.Today)
            yield return new ValidationResult(
                "O prazo não pode estar no passado.", [nameof(Prazo)]);

        if (Prioridade == Prioridade.Alta && Prazo.Date > DateTime.Today.AddDays(3))
            yield return new ValidationResult(
                "Tarefas de prioridade Alta precisam de prazo de até 3 dias.", [nameof(Prazo)]);
    }
}
