using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class BlocoConfiguration : IEntityTypeConfiguration<Bloco>
{
    public void Configure(EntityTypeBuilder<Bloco> builder)
    {
        builder.ToTable("blocos");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(b => b.CondominioId).HasColumnName("condominio_id").IsRequired();
        builder.Property(b => b.Nome).HasColumnName("nome").IsRequired();
        builder.Property(b => b.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");

        builder.HasIndex(b => b.CondominioId);

        builder.HasOne(b => b.Condominio)
            .WithMany(c => c.Blocos)
            .HasForeignKey(b => b.CondominioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
