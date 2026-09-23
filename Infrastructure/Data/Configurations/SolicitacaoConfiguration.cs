using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class SolicitacaoConfiguration : IEntityTypeConfiguration<Solicitacao>
{
    public void Configure(EntityTypeBuilder<Solicitacao> builder)
    {
        builder.UseTptMappingStrategy();

        builder.ToTable("solicitacoes", t =>
        {
            t.HasCheckConstraint("ck_solicitacoes_tipo", "tipo IN ('compra', 'manutencao')");
            t.HasCheckConstraint("ck_solicitacoes_status",
                "status IN ('nova', 'em_andamento', 'finalizada', 'cancelada')");
        });

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.CondominioId).HasColumnName("condominio_id").IsRequired();
        builder.Property(s => s.SolicitadoPorId).HasColumnName("solicitado_por_id").IsRequired();
        builder.Property(s => s.Tipo).HasColumnName("tipo").IsRequired();
        builder.Property(s => s.Status).HasColumnName("status").HasDefaultValue("nova");
        builder.Property(s => s.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        builder.Property(s => s.AtualizadoEm).HasColumnName("atualizado_em");

        builder.HasIndex(s => s.CondominioId);
        builder.HasIndex(s => new { s.CondominioId, s.Status });
        builder.HasIndex(s => s.SolicitadoPorId);

        builder.HasOne(s => s.Condominio)
            .WithMany(c => c.Solicitacoes)
            .HasForeignKey(s => s.CondominioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SolicitadoPor)
            .WithMany()
            .HasForeignKey(s => s.SolicitadoPorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
