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
        });

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(m => m.OcorrenciaId).HasColumnName("ocorrencia_id").IsRequired();
        builder.Property(m => m.UrlArquivo).HasColumnName("url_arquivo").IsRequired();
        builder.Property(m => m.TipoArquivo).HasColumnName("tipo_arquivo").IsRequired();
        builder.Property(m => m.EnviadoPorId).HasColumnName("enviado_por_id").IsRequired();
        builder.Property(m => m.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");

        builder.HasIndex(m => m.OcorrenciaId);

        builder.HasOne(m => m.Ocorrencia)
            .WithMany(o => o.Midias)
            .HasForeignKey(m => m.OcorrenciaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.EnviadoPorId);

        builder.HasOne(m => m.EnviadoPor)
            .WithMany()
            .HasForeignKey(m => m.EnviadoPorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
