using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class MoradorUnidadeConfiguration : IEntityTypeConfiguration<MoradorUnidade>
{
    public void Configure(EntityTypeBuilder<MoradorUnidade> builder)
    {
        builder.ToTable("morador_unidades", t =>
        {
            t.HasCheckConstraint(
                "ck_morador_unidades_papel",
                "papel IN ('proprietario', 'inquilino', 'ocupante')");
        });

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(o => o.MoradorId).HasColumnName("morador_id").IsRequired();
        builder.Property(o => o.UnidadeId).HasColumnName("unidade_id").IsRequired();
        builder.Property(o => o.Papel).HasColumnName("papel").IsRequired();
        builder.Property(o => o.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        builder.Property(o => o.AtualizadoEm).HasColumnName("atualizado_em");
        builder.Property(o => o.DeletadoEm).HasColumnName("deletado_em");

        builder.HasIndex(o => o.MoradorId);
        builder.HasIndex(o => new { o.UnidadeId, o.MoradorId })
            .IsUnique()
            .HasFilter("deletado_em IS NULL")
            .HasDatabaseName("ix_morador_unidades_unidade_morador");
        builder.HasIndex(o => o.UnidadeId)
            .IsUnique()
            .HasFilter("papel = 'proprietario' AND deletado_em IS NULL")
            .HasDatabaseName("ix_morador_unidades_proprietario_unico");

        builder.HasOne(o => o.Morador)
            .WithMany(m => m.Unidades)
            .HasForeignKey(o => o.MoradorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Unidade)
            .WithMany(u => u.MoradorUnidades)
            .HasForeignKey(o => o.UnidadeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
