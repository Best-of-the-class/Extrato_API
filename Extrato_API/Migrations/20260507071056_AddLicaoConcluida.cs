using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Extrato_API.Migrations
{
    /// <inheritdoc />
    public partial class AddLicaoConcluida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            RenomearTabelaUsuariosUp(migrationBuilder);

            migrationBuilder.CreateTable(
                name: "tb_licao_concluida",
                columns: table => new
                {
                    id_licao_concluida = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    licao_id = table.Column<int>(type: "integer", nullable: false),
                    concluido_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_licao_concluida", x => x.id_licao_concluida);
                });

            migrationBuilder.CreateTable(
                name: "tb_modulo",
                columns: table => new
                {
                    id_modulo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    titulo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nivel = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_modulo", x => x.id_modulo);
                });

            migrationBuilder.CreateTable(
                name: "tb_licao",
                columns: table => new
                {
                    id_licao = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    modulo_id = table.Column<int>(type: "integer", nullable: false),
                    titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    recompensa_xp = table.Column<int>(type: "integer", nullable: false),
                    titulo_conceito = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    texto_conceito = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_licao", x => x.id_licao);
                    table.ForeignKey(
                        name: "FK_tb_licao_tb_modulo_modulo_id",
                        column: x => x.modulo_id,
                        principalTable: "tb_modulo",
                        principalColumn: "id_modulo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_atividade",
                columns: table => new
                {
                    id_atividade = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    licao_id = table.Column<int>(type: "integer", nullable: false),
                    enunciado = table.Column<string>(type: "text", nullable: false),
                    dificuldade = table.Column<int>(type: "integer", nullable: false),
                    ordem = table.Column<int>(type: "integer", nullable: false),
                    prova_final = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_atividade", x => x.id_atividade);
                    table.ForeignKey(
                        name: "FK_tb_atividade_tb_licao_licao_id",
                        column: x => x.licao_id,
                        principalTable: "tb_licao",
                        principalColumn: "id_licao",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_alternativa",
                columns: table => new
                {
                    id_alternativa = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    atividade_id = table.Column<int>(type: "integer", nullable: false),
                    texto = table.Column<string>(type: "text", nullable: false),
                    correta = table.Column<bool>(type: "boolean", nullable: true),
                    ordem = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_alternativa", x => x.id_alternativa);
                    table.ForeignKey(
                        name: "FK_tb_alternativa_tb_atividade_atividade_id",
                        column: x => x.atividade_id,
                        principalTable: "tb_atividade",
                        principalColumn: "id_atividade",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_tentativa",
                columns: table => new
                {
                    id_tentativa = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    estudante_id = table.Column<Guid>(type: "uuid", nullable: false),
                    atividade_id = table.Column<int>(type: "integer", nullable: false),
                    alternativa_escolhida_id = table.Column<int>(type: "integer", nullable: true),
                    correta = table.Column<bool>(type: "boolean", nullable: false),
                    xp_ganho = table.Column<int>(type: "integer", nullable: true),
                    tentado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_tentativa", x => x.id_tentativa);
                    table.ForeignKey(
                        name: "FK_tb_tentativa_tb_alternativa_alternativa_escolhida_id",
                        column: x => x.alternativa_escolhida_id,
                        principalTable: "tb_alternativa",
                        principalColumn: "id_alternativa");
                    table.ForeignKey(
                        name: "FK_tb_tentativa_tb_atividade_atividade_id",
                        column: x => x.atividade_id,
                        principalTable: "tb_atividade",
                        principalColumn: "id_atividade",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_alternativa_atividade_id",
                table: "tb_alternativa",
                column: "atividade_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_atividade_licao_id",
                table: "tb_atividade",
                column: "licao_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_licao_modulo_id",
                table: "tb_licao",
                column: "modulo_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_tentativa_alternativa_escolhida_id",
                table: "tb_tentativa",
                column: "alternativa_escolhida_id");

            migrationBuilder.CreateIndex(
                name: "IX_tb_tentativa_atividade_id",
                table: "tb_tentativa",
                column: "atividade_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_licao_concluida");

            migrationBuilder.DropTable(
                name: "tb_tentativa");

            migrationBuilder.DropTable(
                name: "tb_alternativa");

            migrationBuilder.DropTable(
                name: "tb_atividade");

            migrationBuilder.DropTable(
                name: "tb_licao");

            migrationBuilder.DropTable(
                name: "tb_modulo");

            RenomearTabelaUsuariosDown(migrationBuilder);
        }

        // Bloco de renomeação/adaptação da tabela Usuarios para Up
        private void RenomearTabelaUsuariosUp(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Usuarios",
                table: "Usuarios");
            migrationBuilder.DropColumn(
                name: "AvatarId",
                table: "Usuarios");
            migrationBuilder.DropColumn(
                name: "CodigoResetExpiracao",
                table: "Usuarios");
            migrationBuilder.DropColumn(
                name: "CodigoResetSenha",
                table: "Usuarios");
            migrationBuilder.RenameTable(
                name: "Usuarios",
                newName: "tb_usuario");
            migrationBuilder.RenameColumn(
                name: "Email",
                table: "tb_usuario",
                newName: "email");
            migrationBuilder.RenameColumn(
                name: "SenhaHash",
                table: "tb_usuario",
                newName: "senha");
            migrationBuilder.RenameColumn(
                name: "NomeUsuario",
                table: "tb_usuario",
                newName: "nome");
            migrationBuilder.RenameColumn(
                name: "DataCadastro",
                table: "tb_usuario",
                newName: "criado_em");
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "tb_usuario",
                newName: "id_usuario");
            migrationBuilder.AddColumn<string>(
                name: "tipo_usuario",
                table: "tb_usuario",
                type: "text",
                nullable: false,
                defaultValue: "");
            migrationBuilder.AddPrimaryKey(
                name: "PK_tb_usuario",
                table: "tb_usuario",
                column: "id_usuario");
        }

        // Bloco de renomeação/adaptação da tabela Usuarios para Down
        private void RenomearTabelaUsuariosDown(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_tb_usuario",
                table: "tb_usuario");
            migrationBuilder.DropColumn(
                name: "tipo_usuario",
                table: "tb_usuario");
            migrationBuilder.RenameTable(
                name: "tb_usuario",
                newName: "Usuarios");
            migrationBuilder.RenameColumn(
                name: "email",
                table: "Usuarios",
                newName: "Email");
            migrationBuilder.RenameColumn(
                name: "senha",
                table: "Usuarios",
                newName: "SenhaHash");
            migrationBuilder.RenameColumn(
                name: "nome",
                table: "Usuarios",
                newName: "NomeUsuario");
            migrationBuilder.RenameColumn(
                name: "criado_em",
                table: "Usuarios",
                newName: "DataCadastro");
            migrationBuilder.RenameColumn(
                name: "id_usuario",
                table: "Usuarios",
                newName: "Id");
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
    }
}
