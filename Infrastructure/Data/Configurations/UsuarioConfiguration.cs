using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.Nome).HasColumnName("nome").IsRequired();
        builder.Property(u => u.Email).HasColumnName("email").IsRequired();
        builder.Property(u => u.Telefone).HasColumnName("telefone");
        builder.Property(u => u.Ativo).HasColumnName("ativo").HasDefaultValue(true);
        builder.Property(u => u.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        builder.Property(u => u.AtualizadoEm).HasColumnName("atualizado_em");

        builder.HasIndex(u => u.Email);
    }
}
