using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class PessoaConfiguration : IEntityTypeConfiguration<Pessoa>
{
    public void Configure(EntityTypeBuilder<Pessoa> builder)
    {
        builder.ToTable("pessoas");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(p => p.Nome).HasColumnName("nome").IsRequired();
        builder.Property(p => p.Email).HasColumnName("email").IsRequired();
        builder.Property(p => p.Telefone).HasColumnName("telefone");
        builder.Property(p => p.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        builder.Property(p => p.AtualizadoEm).HasColumnName("atualizado_em");
    }
}
