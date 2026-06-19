using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SindiOps.API.Migrations
{
    /// <inheritdoc />
    public partial class EmailLogAutorSindicoOuFuncionario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_email_logs_funcionarios_enviado_por",
                table: "email_logs");

            migrationBuilder.DropIndex(
                name: "IX_email_logs_enviado_por",
                table: "email_logs");

            migrationBuilder.RenameColumn(
                name: "enviado_por",
                table: "email_logs",
                newName: "enviado_funcionario_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "enviado_funcionario_id",
                table: "email_logs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "enviado_sindico_id",
                table: "email_logs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_email_logs_enviado_funcionario_id",
                table: "email_logs",
                column: "enviado_funcionario_id");

            migrationBuilder.CreateIndex(
                name: "IX_email_logs_enviado_sindico_id",
                table: "email_logs",
                column: "enviado_sindico_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_email_logs_enviado_xor",
                table: "email_logs",
                sql: "(enviado_funcionario_id IS NOT NULL AND enviado_sindico_id IS NULL) OR (enviado_funcionario_id IS NULL AND enviado_sindico_id IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_email_logs_funcionarios_enviado_funcionario_id",
                table: "email_logs",
                column: "enviado_funcionario_id",
                principalTable: "funcionarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_email_logs_sindicos_enviado_sindico_id",
                table: "email_logs",
                column: "enviado_sindico_id",
                principalTable: "sindicos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_email_logs_funcionarios_enviado_funcionario_id",
                table: "email_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_email_logs_sindicos_enviado_sindico_id",
                table: "email_logs");

            migrationBuilder.DropIndex(
                name: "IX_email_logs_enviado_funcionario_id",
                table: "email_logs");

            migrationBuilder.DropIndex(
                name: "IX_email_logs_enviado_sindico_id",
                table: "email_logs");

            migrationBuilder.DropCheckConstraint(
                name: "ck_email_logs_enviado_xor",
                table: "email_logs");

            migrationBuilder.DropColumn(
                name: "enviado_sindico_id",
                table: "email_logs");

            migrationBuilder.AlterColumn<Guid>(
                name: "enviado_funcionario_id",
                table: "email_logs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.RenameColumn(
                name: "enviado_funcionario_id",
                table: "email_logs",
                newName: "enviado_por");

            migrationBuilder.CreateIndex(
                name: "IX_email_logs_enviado_por",
                table: "email_logs",
                column: "enviado_por");

            migrationBuilder.AddForeignKey(
                name: "FK_email_logs_funcionarios_enviado_por",
                table: "email_logs",
                column: "enviado_por",
                principalTable: "funcionarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
