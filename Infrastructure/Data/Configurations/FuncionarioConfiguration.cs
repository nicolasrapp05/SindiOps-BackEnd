using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class FuncionarioConfiguration : IEntityTypeConfiguration<Funcionario>
{
    public void Configure(EntityTypeBuilder<Funcionario> builder)
    {
        builder.ToTable("funcionarios", t =>
        {
            t.HasCheckConstraint(
                "ck_funcionarios_cargo",
                "cargo IN ('secretario', 'zelador', 'porteiro', 'outro')");
        });

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(f => f.UsuarioId).HasColumnName("usuario_id").IsRequired();
        builder.Property(f => f.SindicoId).HasColumnName("sindico_id").IsRequired();
        builder.Property(f => f.Cargo).HasColumnName("cargo").IsRequired();
        builder.Property(f => f.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        builder.Property(f => f.AtualizadoEm).HasColumnName("atualizado_em");

        builder.HasIndex(f => f.UsuarioId).IsUnique();
        builder.HasIndex(f => f.SindicoId);

        builder.HasOne(f => f.Usuario)
            .WithOne(u => u.Funcionario)
            .HasForeignKey<Funcionario>(f => f.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Sindico)
            .WithMany(s => s.Equipe)
            .HasForeignKey(f => f.SindicoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
