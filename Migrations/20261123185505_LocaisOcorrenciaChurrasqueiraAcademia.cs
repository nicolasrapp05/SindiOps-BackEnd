using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SindiOps.API.Migrations
{
    /// <inheritdoc />
    public partial class LocaisOcorrenciaChurrasqueiraAcademia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_ocorrencias_tipo_local",
                table: "ocorrencias");

            migrationBuilder.AddCheckConstraint(
                name: "ck_ocorrencias_tipo_local",
                table: "ocorrencias",
                sql: "tipo_local IS NULL OR tipo_local IN ('area_comum','estacionamento','portaria','jardim','salao_festas','hall','corredores','vizinhos','churrasqueira','academia','outro')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_ocorrencias_tipo_local",
                table: "ocorrencias");

            migrationBuilder.AddCheckConstraint(
                name: "ck_ocorrencias_tipo_local",
                table: "ocorrencias",
                sql: "tipo_local IS NULL OR tipo_local IN ('area_comum','estacionamento','portaria','jardim','salao_festas','hall','corredores','vizinhos','outro')");
        }
    }
}
