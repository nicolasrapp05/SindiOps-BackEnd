using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiCore.API.Entities;

namespace SindiCore.API.Infrastructure.Data.Configurations;

public class MoradorConfiguration : IEntityTypeConfiguration<Morador>
{
    public void Configure(EntityTypeBuilder<Morador> builder)
    {
        builder.ToTable("moradores");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(m => m.CondominioId).HasColumnName("condominio_id").IsRequired();
        builder.Property(m => m.BlocoId).HasColumnName("bloco_id").IsRequired();
        builder.Property(m => m.UnidadeId).HasColumnName("unidade_id").IsRequired();
        builder.Property(m => m.Nome).HasColumnName("nome").IsRequired();
        builder.Property(m => m.Email).HasColumnName("email").IsRequired();
        builder.Property(m => m.Telefone).HasColumnName("telefone");
        builder.Property(m => m.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        builder.Property(m => m.AtualizadoEm).HasColumnName("atualizado_em");
        builder.Property(m => m.DeletadoEm).HasColumnName("deletado_em");

        builder.HasIndex(m => m.CondominioId);
        builder.HasIndex(m => m.BlocoId);
        builder.HasIndex(m => m.UnidadeId);

        builder.HasOne(m => m.Condominio)
            .WithMany(c => c.Moradores)
            .HasForeignKey(m => m.CondominioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Bloco)
            .WithMany(b => b.Moradores)
            .HasForeignKey(m => m.BlocoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Unidade)
            .WithMany(u => u.Moradores)
            .HasForeignKey(m => m.UnidadeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
