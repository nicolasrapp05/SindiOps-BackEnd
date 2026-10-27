using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class MoradorConfiguration : IEntityTypeConfiguration<Morador>
{
    public void Configure(EntityTypeBuilder<Morador> builder)
    {
        builder.ToTable("moradores");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(m => m.UsuarioId).HasColumnName("usuario_id").IsRequired();
        builder.Property(m => m.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        builder.Property(m => m.AtualizadoEm).HasColumnName("atualizado_em");

        builder.HasIndex(m => m.UsuarioId).IsUnique();

        builder.HasOne(m => m.Usuario)
            .WithOne(u => u.Morador)
            .HasForeignKey<Morador>(m => m.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
