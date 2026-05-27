using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class FuncionarioConfiguration : IEntityTypeConfiguration<Funcionario>
{
    public void Configure(EntityTypeBuilder<Funcionario> builder)
    {
        builder.ToTable("funcionarios", t =>
            t.HasCheckConstraint("ck_funcionarios_cargo",
                "cargo IN ('zelador', 'secretario', 'porteiro', 'outro')"));

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(f => f.SindicoId).HasColumnName("sindico_id").IsRequired();
        builder.Property(f => f.Nome).HasColumnName("nome").IsRequired();
        builder.Property(f => f.Email).HasColumnName("email").IsRequired();
        builder.Property(f => f.Cargo).HasColumnName("cargo").IsRequired();
        builder.Property(f => f.SenhaHash).HasColumnName("senha_hash").IsRequired();
        builder.Property(f => f.Ativo).HasColumnName("ativo").HasDefaultValue(true);
        builder.Property(f => f.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        builder.Property(f => f.AtualizadoEm).HasColumnName("atualizado_em");

        builder.HasIndex(f => f.Email).IsUnique();
        builder.HasIndex(f => f.SindicoId);

        builder.HasOne(f => f.Sindico)
            .WithMany(s => s.Funcionarios)
            .HasForeignKey(f => f.SindicoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
