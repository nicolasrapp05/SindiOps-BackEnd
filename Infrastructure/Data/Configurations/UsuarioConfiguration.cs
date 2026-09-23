using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios", t =>
        {
            t.HasCheckConstraint("ck_usuarios_cargo",
                "cargo IN ('sindico', 'secretario', 'zelador', 'porteiro', 'outro')");
            t.HasCheckConstraint("ck_usuarios_sindico_carteira",
                "(cargo = 'sindico' AND sindico_id IS NULL) OR (cargo <> 'sindico' AND sindico_id IS NOT NULL)");
        });

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.PessoaId).HasColumnName("pessoa_id").IsRequired();
        builder.Property(u => u.SindicoId).HasColumnName("sindico_id");
        builder.Property(u => u.Cargo).HasColumnName("cargo").IsRequired();
        builder.Property(u => u.Ativo).HasColumnName("ativo").HasDefaultValue(true);
        builder.Property(u => u.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        builder.Property(u => u.AtualizadoEm).HasColumnName("atualizado_em");

        builder.HasIndex(u => u.PessoaId).IsUnique();
        builder.HasIndex(u => u.SindicoId);

        builder.HasOne(u => u.Pessoa)
            .WithOne(p => p.Usuario)
            .HasForeignKey<Usuario>(u => u.PessoaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.Sindico)
            .WithMany(u => u.Equipe)
            .HasForeignKey(u => u.SindicoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
