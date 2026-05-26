using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SindiCore.API.Migrations
{
    /// <inheritdoc />
    public partial class OcorrenciaManutencaoAutorSindicoOuFuncionario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ocorrencias_funcionarios_registrado_por",
                table: "ocorrencias");

            migrationBuilder.DropForeignKey(
                name: "FK_midias_ocorrencia_funcionarios_enviado_por",
                table: "midias_ocorrencia");

            migrationBuilder.DropForeignKey(
                name: "FK_solicitacoes_manutencao_funcionarios_solicitado_por",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropIndex(
                name: "IX_ocorrencias_registrado_por",
                table: "ocorrencias");

            migrationBuilder.DropIndex(
                name: "IX_midias_ocorrencia_enviado_por",
                table: "midias_ocorrencia");

            migrationBuilder.DropIndex(
                name: "IX_solicitacoes_manutencao_solicitado_por",
                table: "solicitacoes_manutencao");

            migrationBuilder.RenameColumn(
                name: "registrado_por",
                table: "ocorrencias",
                newName: "registrado_funcionario_id");

            migrationBuilder.RenameColumn(
                name: "enviado_por",
                table: "midias_ocorrencia",
                newName: "enviado_funcionario_id");

            migrationBuilder.RenameColumn(
                name: "solicitado_por",
                table: "solicitacoes_manutencao",
                newName: "solicitado_funcionario_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "registrado_funcionario_id",
                table: "ocorrencias",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "enviado_funcionario_id",
                table: "midias_ocorrencia",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "solicitado_funcionario_id",
                table: "solicitacoes_manutencao",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "registrado_sindico_id",
                table: "ocorrencias",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "enviado_sindico_id",
                table: "midias_ocorrencia",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "solicitado_sindico_id",
                table: "solicitacoes_manutencao",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ocorrencias_registrado_funcionario_id",
                table: "ocorrencias",
                column: "registrado_funcionario_id");

            migrationBuilder.CreateIndex(
                name: "IX_ocorrencias_registrado_sindico_id",
                table: "ocorrencias",
                column: "registrado_sindico_id");

            migrationBuilder.CreateIndex(
                name: "IX_midias_ocorrencia_enviado_funcionario_id",
                table: "midias_ocorrencia",
                column: "enviado_funcionario_id");

            migrationBuilder.CreateIndex(
                name: "IX_midias_ocorrencia_enviado_sindico_id",
                table: "midias_ocorrencia",
                column: "enviado_sindico_id");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_manutencao_solicitado_funcionario_id",
                table: "solicitacoes_manutencao",
                column: "solicitado_funcionario_id");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_manutencao_solicitado_sindico_id",
                table: "solicitacoes_manutencao",
                column: "solicitado_sindico_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_ocorrencias_registrado_xor",
                table: "ocorrencias",
                sql: "(registrado_funcionario_id IS NOT NULL AND registrado_sindico_id IS NULL) OR (registrado_funcionario_id IS NULL AND registrado_sindico_id IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_midias_enviado_xor",
                table: "midias_ocorrencia",
                sql: "(enviado_funcionario_id IS NOT NULL AND enviado_sindico_id IS NULL) OR (enviado_funcionario_id IS NULL AND enviado_sindico_id IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_sol_manutencao_solicitado_xor",
                table: "solicitacoes_manutencao",
                sql: "(solicitado_funcionario_id IS NOT NULL AND solicitado_sindico_id IS NULL) OR (solicitado_funcionario_id IS NULL AND solicitado_sindico_id IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_ocorrencias_funcionarios_registrado_funcionario_id",
                table: "ocorrencias",
                column: "registrado_funcionario_id",
                principalTable: "funcionarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ocorrencias_sindicos_registrado_sindico_id",
                table: "ocorrencias",
                column: "registrado_sindico_id",
                principalTable: "sindicos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_midias_ocorrencia_funcionarios_enviado_funcionario_id",
                table: "midias_ocorrencia",
                column: "enviado_funcionario_id",
                principalTable: "funcionarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_midias_ocorrencia_sindicos_enviado_sindico_id",
                table: "midias_ocorrencia",
                column: "enviado_sindico_id",
                principalTable: "sindicos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_solicitacoes_manutencao_funcionarios_solicitado_funcionario_id",
                table: "solicitacoes_manutencao",
                column: "solicitado_funcionario_id",
                principalTable: "funcionarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_solicitacoes_manutencao_sindicos_solicitado_sindico_id",
                table: "solicitacoes_manutencao",
                column: "solicitado_sindico_id",
                principalTable: "sindicos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ocorrencias_funcionarios_registrado_funcionario_id",
                table: "ocorrencias");

            migrationBuilder.DropForeignKey(
                name: "FK_ocorrencias_sindicos_registrado_sindico_id",
                table: "ocorrencias");

            migrationBuilder.DropForeignKey(
                name: "FK_midias_ocorrencia_funcionarios_enviado_funcionario_id",
                table: "midias_ocorrencia");

            migrationBuilder.DropForeignKey(
                name: "FK_midias_ocorrencia_sindicos_enviado_sindico_id",
                table: "midias_ocorrencia");

            migrationBuilder.DropForeignKey(
                name: "FK_solicitacoes_manutencao_funcionarios_solicitado_funcionario_id",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropForeignKey(
                name: "FK_solicitacoes_manutencao_sindicos_solicitado_sindico_id",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropIndex(
                name: "IX_ocorrencias_registrado_funcionario_id",
                table: "ocorrencias");

            migrationBuilder.DropIndex(
                name: "IX_ocorrencias_registrado_sindico_id",
                table: "ocorrencias");

            migrationBuilder.DropIndex(
                name: "IX_midias_ocorrencia_enviado_funcionario_id",
                table: "midias_ocorrencia");

            migrationBuilder.DropIndex(
                name: "IX_midias_ocorrencia_enviado_sindico_id",
                table: "midias_ocorrencia");

            migrationBuilder.DropIndex(
                name: "IX_solicitacoes_manutencao_solicitado_funcionario_id",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropIndex(
                name: "IX_solicitacoes_manutencao_solicitado_sindico_id",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropCheckConstraint(
                name: "ck_ocorrencias_registrado_xor",
                table: "ocorrencias");

            migrationBuilder.DropCheckConstraint(
                name: "ck_midias_enviado_xor",
                table: "midias_ocorrencia");

            migrationBuilder.DropCheckConstraint(
                name: "ck_sol_manutencao_solicitado_xor",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropColumn(
                name: "registrado_sindico_id",
                table: "ocorrencias");

            migrationBuilder.DropColumn(
                name: "enviado_sindico_id",
                table: "midias_ocorrencia");

            migrationBuilder.DropColumn(
                name: "solicitado_sindico_id",
                table: "solicitacoes_manutencao");

            migrationBuilder.RenameColumn(
                name: "registrado_funcionario_id",
                table: "ocorrencias",
                newName: "registrado_por");

            migrationBuilder.RenameColumn(
                name: "enviado_funcionario_id",
                table: "midias_ocorrencia",
                newName: "enviado_por");

            migrationBuilder.RenameColumn(
                name: "solicitado_funcionario_id",
                table: "solicitacoes_manutencao",
                newName: "solicitado_por");

            migrationBuilder.AlterColumn<Guid>(
                name: "registrado_por",
                table: "ocorrencias",
                type: "uuid",
                nullable: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "enviado_por",
                table: "midias_ocorrencia",
                type: "uuid",
                nullable: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "solicitado_por",
                table: "solicitacoes_manutencao",
                type: "uuid",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_ocorrencias_registrado_por",
                table: "ocorrencias",
                column: "registrado_por");

            migrationBuilder.CreateIndex(
                name: "IX_midias_ocorrencia_enviado_por",
                table: "midias_ocorrencia",
                column: "enviado_por");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_manutencao_solicitado_por",
                table: "solicitacoes_manutencao",
                column: "solicitado_por");

            migrationBuilder.AddForeignKey(
                name: "FK_ocorrencias_funcionarios_registrado_por",
                table: "ocorrencias",
                column: "registrado_por",
                principalTable: "funcionarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_midias_ocorrencia_funcionarios_enviado_por",
                table: "midias_ocorrencia",
                column: "enviado_por",
                principalTable: "funcionarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_solicitacoes_manutencao_funcionarios_solicitado_por",
                table: "solicitacoes_manutencao",
                column: "solicitado_por",
                principalTable: "funcionarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
