namespace PortalTarefas.Web.Models;

// Projeção para o painel: só as 4 colunas que a tela precisa.
// Usada pelas DUAS versões da consulta (EF Core e Dapper).
public class PainelItem
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public Prioridade Prioridade { get; set; }
    public DateTime Prazo { get; set; }
}
