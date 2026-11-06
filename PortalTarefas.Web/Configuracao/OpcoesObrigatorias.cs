using System.ComponentModel.DataAnnotations;

namespace PortalTarefas.Web.Configuracao;

// Roteiro 8 — as configurações SEM as quais a aplicação não deve nem iniciar.
// As mensagens dizem QUAL chave falta e ONDE defini-la — nunca mostram valores.

// Seção "ConnectionStrings"
public class ConexoesOpcoes
{
    [Required(ErrorMessage = "Configuração obrigatória ausente: ConnectionStrings:PortalTarefasDb. " +
        "Em desenvolvimento, use User Secrets; fora dele, a variável de ambiente ConnectionStrings__PortalTarefasDb.")]
    public string? PortalTarefasDb { get; set; }
}

// Seção "Externo"
public class ExternoOpcoes
{
    [Required(ErrorMessage = "Configuração obrigatória ausente: Externo:BaseUrl (variável de ambiente Externo__BaseUrl).")]
    [Url(ErrorMessage = "Externo:BaseUrl precisa ser um endereço http:// ou https:// válido.")]
    public string? BaseUrl { get; set; }
}