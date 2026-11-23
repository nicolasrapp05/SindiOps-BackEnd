using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SindiOps.API.Infrastructure.Data;

#nullable disable

namespace SindiOps.API.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(SindiOpsDbContext))]
    [Migration("20261123140000_NormalizarTipoServicoFornecedor")]
    public partial class NormalizarTipoServicoFornecedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE servicos_fornecedor AS s
                SET tipo = rotulo.texto
                FROM (
                    SELECT id,
                           string_agg(
                               upper(left(parte.palavra, 1)) || substring(parte.palavra FROM 2),
                               ' ' ORDER BY parte.ordem
                           ) AS texto
                    FROM servicos_fornecedor
                    CROSS JOIN LATERAL regexp_split_to_table(btrim(tipo), '_')
                        WITH ORDINALITY AS parte(palavra, ordem)
                    WHERE btrim(tipo) ~ '^[a-z0-9]+(_[a-z0-9]+)+$'
                    GROUP BY id
                ) AS rotulo
                WHERE s.id = rotulo.id;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Texto livre já formatado não guarda o underscore original.
        }
    }
}
