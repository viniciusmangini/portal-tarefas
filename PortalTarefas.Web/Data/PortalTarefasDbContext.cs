using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortalTarefas.Web.Models;

namespace PortalTarefas.Web.Data;

// Roteiro 6: herda de IdentityDbContext, que acrescenta as tabelas de
// usuários, papéis (roles) e claims (AspNetUsers, AspNetRoles, AspNetUserClaims...).
public class PortalTarefasDbContext(DbContextOptions<PortalTarefasDbContext> options)
    : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<Tarefa> Tarefas => Set<Tarefa>();
    public DbSet<EventoAuditoria> EventosAuditoria => Set<EventoAuditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // Roteiro 6: obrigatório! Configura as tabelas do Identity

        modelBuilder.Entity<Tarefa>(tarefa =>
        {
            tarefa.HasKey(t => t.Id);
            tarefa.Property(t => t.Titulo).IsRequired().HasMaxLength(120);
            tarefa.Property(t => t.Descricao).HasMaxLength(500);
            tarefa.HasIndex(t => t.Prazo);
            tarefa.Property(t => t.RowVersion).IsConcurrencyToken();
            tarefa.Property(t => t.Equipe).HasMaxLength(30);            // Roteiro 6

            // SEED: valores FIXOS (nada de DateTime.Today nem Guid.NewGuid() aqui)
            tarefa.HasData(
                new Tarefa
                {
                    Id = 1, Titulo = "Configurar ambiente", Situacao = Situacao.Concluida,
                    Prioridade = Prioridade.Alta, Prazo = new DateTime(2026, 9, 15),
                    RowVersion = "seed-1", Equipe = "Alfa"
                },
                new Tarefa
                {
                    Id = 2, Titulo = "Revisar documentação", Situacao = Situacao.Pendente,
                    Prioridade = Prioridade.Baixa, Prazo = new DateTime(2026, 12, 15),
                    RowVersion = "seed-2", Equipe = "Alfa"
                },
                new Tarefa
                {
                    Id = 3, Titulo = "Preparar apresentação", Situacao = Situacao.EmAndamento,
                    Prioridade = Prioridade.Media, Prazo = new DateTime(2026, 11, 30),
                    RowVersion = "seed-3", Equipe = "Beta"
                });
        });

        modelBuilder.Entity<EventoAuditoria>(evento =>
            evento.Property(e => e.Acao).IsRequired().HasMaxLength(50));
    }
}