using System.ComponentModel.DataAnnotations;
using PortalTarefas.Web.Models;

namespace PortalTarefas.Testes;

// Roteiro 8 — testes que o PIPELINE roda a cada push. Se um falhar, nada é publicado.
// Testam as regras de validação do formulário de tarefas (Roteiro 2).
public class TarefaInputModelTestes
{
    // Roda a MESMA validação que o MVC roda: as anotações e o Validate() do modelo
    private static List<ValidationResult> Validar(TarefaInputModel modelo)
    {
        var erros = new List<ValidationResult>();
        Validator.TryValidateObject(modelo, new ValidationContext(modelo), erros, validateAllProperties: true);
        return erros;
    }

    [Fact]   // [Fact] = um teste
    public void Tarefa_valida_nao_tem_erros()
    {
        var modelo = new TarefaInputModel
        {
            Titulo = "Estudar Docker", Prioridade = Prioridade.Media, Prazo = DateTime.Today.AddDays(5)
        };

        Assert.NotEmpty(Validar(modelo));   // Assert = "confira que..."; se não for verdade, o teste FALHA
    }

    [Fact]
    public void Prazo_no_passado_e_recusado()
    {
        var modelo = new TarefaInputModel
        {
            Titulo = "Estudar Docker", Prioridade = Prioridade.Media, Prazo = DateTime.Today.AddDays(-1)
        };

        Assert.Contains(Validar(modelo), erro => erro.ErrorMessage == "O prazo não pode estar no passado.");
    }

    [Fact]
    public void Prioridade_alta_exige_prazo_de_ate_3_dias()
    {
        var modelo = new TarefaInputModel
        {
            Titulo = "Estudar Docker", Prioridade = Prioridade.Alta, Prazo = DateTime.Today.AddDays(10)
        };

        Assert.Contains(Validar(modelo), erro => erro.ErrorMessage == "Tarefas de prioridade Alta precisam de prazo de até 3 dias.");
    }
}