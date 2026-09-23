using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class MoradorConfiguration : IEntityTypeConfiguration<Morador>
{
    public void Configure(EntityTypeBuilder<Morador> builder)
    {
        builder.ToTable("moradores", t =>
        {
            t.HasCheckConstraint("ck_moradores_papel",
                "papel IN ('proprietario', 'inquilino', 'ocupante')");
        });

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(m => m.PessoaId).HasColumnName("pessoa_id").IsRequired();
        builder.Property(m => m.UnidadeId).HasColumnName("unidade_id").IsRequired();
        builder.Property(m => m.Papel).HasColumnName("papel").IsRequired();
        builder.Property(m => m.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        builder.Property(m => m.AtualizadoEm).HasColumnName("atualizado_em");
        builder.Property(m => m.DeletadoEm).HasColumnName("deletado_em");

        builder.HasIndex(m => m.PessoaId);
        builder.HasIndex(m => m.UnidadeId);
        builder.HasIndex(m => new { m.UnidadeId, m.PessoaId })
            .IsUnique()
            .HasFilter("deletado_em IS NULL")
            .HasDatabaseName("ix_moradores_unidade_pessoa");
        builder.HasIndex(m => m.UnidadeId)
            .IsUnique()
            .HasFilter("papel = 'proprietario' AND deletado_em IS NULL")
            .HasDatabaseName("ix_moradores_proprietario_unico");

        builder.HasOne(m => m.Pessoa)
            .WithMany(p => p.Moradores)
            .HasForeignKey(m => m.PessoaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Unidade)
            .WithMany(u => u.Moradores)
            .HasForeignKey(m => m.UnidadeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
