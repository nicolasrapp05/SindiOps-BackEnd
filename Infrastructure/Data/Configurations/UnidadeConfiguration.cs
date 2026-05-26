using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiCore.API.Entities;

namespace SindiCore.API.Infrastructure.Data.Configurations;

public class UnidadeConfiguration : IEntityTypeConfiguration<Unidade>
{
    public void Configure(EntityTypeBuilder<Unidade> builder)
    {
        builder.ToTable("unidades");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(u => u.BlocoId).HasColumnName("bloco_id").IsRequired();
        builder.Property(u => u.CondominioId).HasColumnName("condominio_id").IsRequired();
        builder.Property(u => u.Numero).HasColumnName("numero").IsRequired();
        builder.Property(u => u.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");

        builder.HasIndex(u => u.BlocoId);
        builder.HasIndex(u => u.CondominioId);

        builder.HasOne(u => u.Bloco)
            .WithMany(b => b.Unidades)
            .HasForeignKey(u => u.BlocoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Condominio)
            .WithMany(c => c.Unidades)
            .HasForeignKey(u => u.CondominioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
