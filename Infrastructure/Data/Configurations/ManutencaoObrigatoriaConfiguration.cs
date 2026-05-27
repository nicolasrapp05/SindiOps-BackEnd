using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class ManutencaoObrigatoriaConfiguration : IEntityTypeConfiguration<ManutencaoObrigatoria>
{
    public void Configure(EntityTypeBuilder<ManutencaoObrigatoria> builder)
    {
        builder.ToTable("manutencoes_obrigatorias", t =>
        {
            t.HasCheckConstraint("ck_manutencoes_status",
                "status IN ('ok', 'upcoming', 'overdue')");
            t.HasCheckConstraint("ck_manutencoes_tipo",
                "tipo IN ('dedetizacao','para_raios','seguro','limpeza_caixa_agua','caixa_gordura_esgoto','extintores','cvcb','calhas_telhado','ppra','pcmso','pgr')");
        });

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(m => m.CondominioId).HasColumnName("condominio_id").IsRequired();
        builder.Property(m => m.Tipo).HasColumnName("tipo").IsRequired();
        builder.Property(m => m.DataVencimento).HasColumnName("data_vencimento").IsRequired();
        builder.Property(m => m.UltimaRealizacao).HasColumnName("ultima_realizacao");
        builder.Property(m => m.Status).HasColumnName("status").HasDefaultValue("ok");
        builder.Property(m => m.Observacoes).HasColumnName("observacoes");
        builder.Property(m => m.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        builder.Property(m => m.AtualizadoEm).HasColumnName("atualizado_em");

        builder.HasIndex(m => m.CondominioId);
        builder.HasIndex(m => m.Status);
        builder.HasIndex(m => new { m.CondominioId, m.DataVencimento });

        builder.HasOne(m => m.Condominio)
            .WithMany(c => c.ManutencoesObrigatorias)
            .HasForeignKey(m => m.CondominioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
