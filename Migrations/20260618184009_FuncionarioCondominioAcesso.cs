using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SindiOps.API.Migrations
{
    /// <inheritdoc />
    public partial class FuncionarioCondominioAcesso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "funcionario_condominios",
                columns: table => new
                {
                    funcionario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    condominio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_funcionario_condominios", x => new { x.funcionario_id, x.condominio_id });
                    table.ForeignKey(
                        name: "FK_funcionario_condominios_condominios_condominio_id",
                        column: x => x.condominio_id,
                        principalTable: "condominios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_funcionario_condominios_funcionarios_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_funcionario_condominios_condominio_id",
                table: "funcionario_condominios",
                column: "condominio_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "funcionario_condominios");
        }
    }
}
