using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Extrato_API.Migrations
{
    /// <inheritdoc />
    public partial class AdicionandoEstatisticasUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EstatisticasUsuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    LicoesConcluidas = table.Column<int>(type: "integer", nullable: false),
                    ExerciciosResolvidos = table.Column<int>(type: "integer", nullable: false),
                    Pontuacao = table.Column<int>(type: "integer", nullable: false),
                    SequenciaDias = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstatisticasUsuarios", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EstatisticasUsuarios");
        }
    }
}
