using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class UsuarioCondominioConfiguration : IEntityTypeConfiguration<UsuarioCondominio>
{
    public void Configure(EntityTypeBuilder<UsuarioCondominio> builder)
    {
        builder.ToTable("usuario_condominios");

        builder.HasKey(uc => new { uc.UsuarioId, uc.CondominioId });

        builder.Property(uc => uc.UsuarioId).HasColumnName("usuario_id");
        builder.Property(uc => uc.CondominioId).HasColumnName("condominio_id");
        builder.Property(uc => uc.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");

        builder.HasIndex(uc => uc.CondominioId);

        builder.HasOne(uc => uc.Usuario)
            .WithMany(u => u.CondominiosAcesso)
            .HasForeignKey(uc => uc.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(uc => uc.Condominio)
            .WithMany(c => c.UsuariosAcesso)
            .HasForeignKey(uc => uc.CondominioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
