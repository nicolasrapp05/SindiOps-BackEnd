using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class FuncionarioCondominioConfiguration : IEntityTypeConfiguration<FuncionarioCondominio>
{
    public void Configure(EntityTypeBuilder<FuncionarioCondominio> builder)
    {
        builder.ToTable("funcionario_condominios");

        builder.HasKey(fc => new { fc.FuncionarioId, fc.CondominioId });

        builder.Property(fc => fc.FuncionarioId).HasColumnName("funcionario_id");
        builder.Property(fc => fc.CondominioId).HasColumnName("condominio_id");
        builder.Property(fc => fc.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");

        builder.HasIndex(fc => fc.CondominioId);

        builder.HasOne(fc => fc.Funcionario)
            .WithMany(f => f.CondominiosAcesso)
            .HasForeignKey(fc => fc.FuncionarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fc => fc.Condominio)
            .WithMany(c => c.FuncionariosAcesso)
            .HasForeignKey(fc => fc.CondominioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
