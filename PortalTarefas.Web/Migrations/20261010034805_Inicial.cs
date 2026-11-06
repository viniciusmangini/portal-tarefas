using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PortalTarefas.Web.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventosAuditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TarefaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Acao = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DataHora = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosAuditoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tarefas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Situacao = table.Column<int>(type: "INTEGER", nullable: false),
                    Prioridade = table.Column<int>(type: "INTEGER", nullable: false),
                    Prazo = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tarefas", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Tarefas",
                columns: new[] { "Id", "Descricao", "Prazo", "Prioridade", "RowVersion", "Situacao", "Titulo" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2026, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "seed-1", 2, "Configurar ambiente" },
                    { 2, null, new DateTime(2026, 12, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 0, "seed-2", 0, "Revisar documentação" },
                    { 3, null, new DateTime(2026, 11, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "seed-3", 1, "Preparar apresentação" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tarefas_Prazo",
                table: "Tarefas",
                column: "Prazo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventosAuditoria");

            migrationBuilder.DropTable(
                name: "Tarefas");
        }
    }
}
