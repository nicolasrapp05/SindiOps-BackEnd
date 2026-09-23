using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class SolicitacaoCompraConfiguration : IEntityTypeConfiguration<SolicitacaoCompra>
{
    public void Configure(EntityTypeBuilder<SolicitacaoCompra> builder)
    {
        builder.ToTable("solicitacoes_compra", t =>
        {
            t.HasCheckConstraint("ck_sol_compra_tipo_aprovacao",
                "tipo_aprovacao IS NULL OR tipo_aprovacao IN ('sindico', 'conselho', 'assembleia')");
        });

        builder.Property(s => s.Justificativa).HasColumnName("justificativa");
        builder.Property(s => s.TipoAprovacao).HasColumnName("tipo_aprovacao");
        builder.Property(s => s.AprovadoPorId).HasColumnName("aprovado_por_id");

        builder.HasIndex(s => s.AprovadoPorId);

        builder.HasOne(s => s.AprovadoPor)
            .WithMany()
            .HasForeignKey(s => s.AprovadoPorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
