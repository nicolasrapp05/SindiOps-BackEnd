using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class SolicitacaoManutencaoConfiguration : IEntityTypeConfiguration<SolicitacaoManutencao>
{
    public void Configure(EntityTypeBuilder<SolicitacaoManutencao> builder)
    {
        builder.ToTable("solicitacoes_manutencao", t =>
        {
            t.HasCheckConstraint("ck_sol_manutencao_tipo",
                "tipo IN ('obra_civil','pintura','serralheria','eletrica','hidraulica','cameras','portas_portoes','jardim','esgoto','caixa_gordura','outro')");
            t.HasCheckConstraint("ck_sol_manutencao_responsavel",
                "responsavel IS NULL OR responsavel IN ('fornecedor', 'zelador')");
        });

        builder.Property(s => s.FornecedorId).HasColumnName("fornecedor_id");
        builder.Property(s => s.Local).HasColumnName("local");
        builder.Property(s => s.TipoManutencao).HasColumnName("tipo").IsRequired();
        builder.Property(s => s.Responsavel).HasColumnName("responsavel");
        builder.Property(s => s.Descricao).HasColumnName("descricao");
        builder.Property(s => s.DataConclusao).HasColumnName("data_conclusao");

        builder.HasIndex(s => s.FornecedorId);

        builder.HasOne(s => s.Fornecedor)
            .WithMany()
            .HasForeignKey(s => s.FornecedorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
