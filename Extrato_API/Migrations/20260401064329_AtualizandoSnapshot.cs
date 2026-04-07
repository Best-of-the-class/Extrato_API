using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Extrato_API.Migrations
{
    /// <inheritdoc />
    public partial class AtualizandoSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AvatarId",
                table: "Usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CodigoResetExpiracao",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoResetSenha",
                table: "Usuarios",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "CodigoResetExpiracao",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "CodigoResetSenha",
                table: "Usuarios");
        }
    }
}
