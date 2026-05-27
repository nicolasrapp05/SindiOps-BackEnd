using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SindiOps.API.Migrations
{
    /// <inheritdoc />
    public partial class SolicitacaoCompraAutorSindicoOuFuncionario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_solicitacoes_compra_funcionarios_solicitado_por",
                table: "solicitacoes_compra");

            migrationBuilder.DropIndex(
                name: "IX_solicitacoes_compra_solicitado_por",
                table: "solicitacoes_compra");

            migrationBuilder.RenameColumn(
                name: "solicitado_por",
                table: "solicitacoes_compra",
                newName: "solicitado_funcionario_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "solicitado_funcionario_id",
                table: "solicitacoes_compra",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "solicitado_sindico_id",
                table: "solicitacoes_compra",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_compra_solicitado_funcionario_id",
                table: "solicitacoes_compra",
                column: "solicitado_funcionario_id");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_compra_solicitado_sindico_id",
                table: "solicitacoes_compra",
                column: "solicitado_sindico_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_sol_compra_solicitado_xor",
                table: "solicitacoes_compra",
                sql: "(solicitado_funcionario_id IS NOT NULL AND solicitado_sindico_id IS NULL) OR (solicitado_funcionario_id IS NULL AND solicitado_sindico_id IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_solicitacoes_compra_funcionarios_solicitado_funcionario_id",
                table: "solicitacoes_compra",
                column: "solicitado_funcionario_id",
                principalTable: "funcionarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_solicitacoes_compra_sindicos_solicitado_sindico_id",
                table: "solicitacoes_compra",
                column: "solicitado_sindico_id",
                principalTable: "sindicos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_solicitacoes_compra_funcionarios_solicitado_funcionario_id",
                table: "solicitacoes_compra");

            migrationBuilder.DropForeignKey(
                name: "FK_solicitacoes_compra_sindicos_solicitado_sindico_id",
                table: "solicitacoes_compra");

            migrationBuilder.DropIndex(
                name: "IX_solicitacoes_compra_solicitado_funcionario_id",
                table: "solicitacoes_compra");

            migrationBuilder.DropIndex(
                name: "IX_solicitacoes_compra_solicitado_sindico_id",
                table: "solicitacoes_compra");

            migrationBuilder.DropCheckConstraint(
                name: "ck_sol_compra_solicitado_xor",
                table: "solicitacoes_compra");

            migrationBuilder.DropColumn(
                name: "solicitado_sindico_id",
                table: "solicitacoes_compra");

            migrationBuilder.RenameColumn(
                name: "solicitado_funcionario_id",
                table: "solicitacoes_compra",
                newName: "solicitado_por");

            migrationBuilder.AlterColumn<Guid>(
                name: "solicitado_por",
                table: "solicitacoes_compra",
                type: "uuid",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_compra_solicitado_por",
                table: "solicitacoes_compra",
                column: "solicitado_por");

            migrationBuilder.AddForeignKey(
                name: "FK_solicitacoes_compra_funcionarios_solicitado_por",
                table: "solicitacoes_compra",
                column: "solicitado_por",
                principalTable: "funcionarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
