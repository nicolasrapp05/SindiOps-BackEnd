using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SindiOps.API.Migrations
{
    /// <inheritdoc />
    public partial class UnidadeNumeroUnicoPorBloco : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_unidades_bloco_id_numero",
                table: "unidades",
                columns: new[] { "bloco_id", "numero" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_unidades_bloco_id_numero",
                table: "unidades");
        }
    }
}
