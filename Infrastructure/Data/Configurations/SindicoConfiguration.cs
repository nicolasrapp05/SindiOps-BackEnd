using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class SindicoConfiguration : IEntityTypeConfiguration<Sindico>
{
    public void Configure(EntityTypeBuilder<Sindico> builder)
    {
        builder.ToTable("sindicos");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.UsuarioId).HasColumnName("usuario_id").IsRequired();
        builder.Property(s => s.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        builder.Property(s => s.AtualizadoEm).HasColumnName("atualizado_em");

        builder.HasIndex(s => s.UsuarioId).IsUnique();

        builder.HasOne(s => s.Usuario)
            .WithOne(u => u.Sindico)
            .HasForeignKey<Sindico>(s => s.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
