using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Extrato_API.Migrations
{
    public partial class AdicionandoResetSenhaEAvatar : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoResetSenha",
                table: "Usuarios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CodigoResetExpiracao",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AvatarId",
                table: "Usuarios",
                type: "integer",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CodigoResetSenha", table: "Usuarios");
            migrationBuilder.DropColumn(name: "CodigoResetExpiracao", table: "Usuarios");
            migrationBuilder.DropColumn(name: "AvatarId", table: "Usuarios");
        }
    }
}