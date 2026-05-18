using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Extrato_API.Migrations
{
    /// <inheritdoc />
    public partial class RenameLicaoConcluidaColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only rename columns in existing tb_licao_concluida table if present
            migrationBuilder.Sql(@"DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'tb_licao_concluida' AND column_name = 'usuario_id'
    ) THEN
        ALTER TABLE tb_licao_concluida RENAME COLUMN usuario_id TO id_usuario;
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'tb_licao_concluida' AND column_name = 'licao_id'
    ) THEN
        ALTER TABLE tb_licao_concluida RENAME COLUMN licao_id TO id_licao;
    END IF;
END
$$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'tb_licao_concluida' AND column_name = 'id_licao'
    ) THEN
        ALTER TABLE tb_licao_concluida RENAME COLUMN id_licao TO licao_id;
    END IF;

    IF EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'tb_licao_concluida' AND column_name = 'id_usuario'
    ) THEN
        ALTER TABLE tb_licao_concluida RENAME COLUMN id_usuario TO usuario_id;
    END IF;
END
$$;");
        }
    }
}
