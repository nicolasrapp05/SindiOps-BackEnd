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
        builder.Property(s => s.Nome).HasColumnName("nome").IsRequired();
        builder.Property(s => s.Email).HasColumnName("email").IsRequired();
        builder.Property(s => s.Telefone).HasColumnName("telefone");
        builder.Property(s => s.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        builder.Property(s => s.AtualizadoEm).HasColumnName("atualizado_em");

        builder.HasIndex(s => s.Email).IsUnique();
    }
}
