using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Extrato_API.Migrations
{
    /// <inheritdoc />
    public partial class SincronizarConquistasExistentes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Intencionalmente vazio: as tabelas de conquistas ja existiam no banco legado.
            // Esta migracao serviu apenas para alinhar o snapshot do EF com o esquema existente.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intencionalmente vazio: nao ha alteracoes estruturais para desfazer nesta sincronizacao.
        }
    }
}
