using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SindiOps.API.Migrations
{
    /// <inheritdoc />
    public partial class OrdemItemSolicitacaoCompra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ordem",
                table: "solicitacao_compra_itens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE solicitacao_compra_itens AS item
                SET ordem = numerado.rn
                FROM (
                    SELECT id, ROW_NUMBER() OVER (PARTITION BY solicitacao_compra_id ORDER BY id) - 1 AS rn
                    FROM solicitacao_compra_itens
                ) AS numerado
                WHERE item.id = numerado.id;
                """);

            migrationBuilder.AddCheckConstraint(
                name: "ck_sol_compra_item_ordem",
                table: "solicitacao_compra_itens",
                sql: "ordem >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_sol_compra_item_ordem",
                table: "solicitacao_compra_itens");

            migrationBuilder.DropColumn(
                name: "ordem",
                table: "solicitacao_compra_itens");
        }
    }
}
