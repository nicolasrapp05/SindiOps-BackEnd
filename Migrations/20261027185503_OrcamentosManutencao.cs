using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SindiOps.API.Migrations
{
    /// <inheritdoc />
    public partial class OrcamentosManutencao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "orcamento_aprovado_por_id",
                table: "solicitacoes_manutencao",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "orcamentos_manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    solicitacao_manutencao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nome_empresa = table.Column<string>(type: "text", nullable: true),
                    nome_contato = table.Column<string>(type: "text", nullable: true),
                    nome_responsavel = table.Column<string>(type: "text", nullable: true),
                    forma_pagamento = table.Column<string>(type: "text", nullable: true),
                    descricao_escopo = table.Column<string>(type: "text", nullable: true),
                    valor = table.Column<decimal>(type: "numeric", nullable: false),
                    prazo_execucao_dias = table.Column<int>(type: "integer", nullable: true),
                    validade_proposta = table.Column<DateOnly>(type: "date", nullable: true),
                    garantia = table.Column<string>(type: "text", nullable: true),
                    observacoes = table.Column<string>(type: "text", nullable: true),
                    selecionada = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orcamentos_manutencao", x => x.id);
                    table.CheckConstraint("ck_orcamento_manutencao_prazo", "prazo_execucao_dias IS NULL OR prazo_execucao_dias > 0");
                    table.CheckConstraint("ck_orcamento_manutencao_valor", "valor > 0");
                    table.ForeignKey(
                        name: "FK_orcamentos_manutencao_fornecedores_fornecedor_id",
                        column: x => x.fornecedor_id,
                        principalTable: "fornecedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_orcamentos_manutencao_solicitacoes_manutencao_solicitacao_m~",
                        column: x => x.solicitacao_manutencao_id,
                        principalTable: "solicitacoes_manutencao",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_manutencao_orcamento_aprovado_por_id",
                table: "solicitacoes_manutencao",
                column: "orcamento_aprovado_por_id");

            migrationBuilder.CreateIndex(
                name: "IX_orcamentos_manutencao_fornecedor_id",
                table: "orcamentos_manutencao",
                column: "fornecedor_id");

            migrationBuilder.CreateIndex(
                name: "ix_orcamentos_manutencao_selecionada_unica",
                table: "orcamentos_manutencao",
                column: "solicitacao_manutencao_id",
                unique: true,
                filter: "selecionada = true");

            migrationBuilder.AddForeignKey(
                name: "FK_solicitacoes_manutencao_usuarios_orcamento_aprovado_por_id",
                table: "solicitacoes_manutencao",
                column: "orcamento_aprovado_por_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_solicitacoes_manutencao_usuarios_orcamento_aprovado_por_id",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropTable(
                name: "orcamentos_manutencao");

            migrationBuilder.DropIndex(
                name: "IX_solicitacoes_manutencao_orcamento_aprovado_por_id",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropColumn(
                name: "orcamento_aprovado_por_id",
                table: "solicitacoes_manutencao");
        }
    }
}
