using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Extrato_API.Migrations
{
    /// <inheritdoc />
    public partial class SincronizarTodasAsTabelas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Tabelas tb_estudante, tb_avatar, tb_conquista, tb_conquista_estudante
            // e tb_dicionario já existem no banco (criadas pelo time de banco).
            // Esta migration sincroniza o snapshot do EF sem executar DDL.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reversão vazia — não removemos tabelas que não criamos aqui.
        }
    }
}