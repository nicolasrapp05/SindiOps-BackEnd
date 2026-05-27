using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class MidiaOcorrenciaConfiguration : IEntityTypeConfiguration<MidiaOcorrencia>
{
    public void Configure(EntityTypeBuilder<MidiaOcorrencia> builder)
    {
        builder.ToTable("midias_ocorrencia", t =>
        {
            t.HasCheckConstraint("ck_midias_tipo_arquivo",
                "tipo_arquivo IN ('image', 'video')");
            t.HasCheckConstraint("ck_midias_enviado_xor",
                "(enviado_funcionario_id IS NOT NULL AND enviado_sindico_id IS NULL) OR (enviado_funcionario_id IS NULL AND enviado_sindico_id IS NOT NULL)");
        });

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(m => m.OcorrenciaId).HasColumnName("ocorrencia_id").IsRequired();
        builder.Property(m => m.UrlArquivo).HasColumnName("url_arquivo").IsRequired();
        builder.Property(m => m.TipoArquivo).HasColumnName("tipo_arquivo").IsRequired();
        builder.Property(m => m.EnviadoPorFuncionarioId).HasColumnName("enviado_funcionario_id");
        builder.Property(m => m.EnviadoPorSindicoId).HasColumnName("enviado_sindico_id");
        builder.Property(m => m.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");

        builder.HasIndex(m => m.OcorrenciaId);

        builder.HasOne(m => m.Ocorrencia)
            .WithMany(o => o.Midias)
            .HasForeignKey(m => m.OcorrenciaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.EnviadoPorFuncionario)
            .WithMany()
            .HasForeignKey(m => m.EnviadoPorFuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.EnviadoPorSindico)
            .WithMany()
            .HasForeignKey(m => m.EnviadoPorSindicoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
