using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class OcorrenciaConfiguration : IEntityTypeConfiguration<Ocorrencia>
{
    public void Configure(EntityTypeBuilder<Ocorrencia> builder)
    {
        builder.ToTable("ocorrencias", t =>
        {
            t.HasCheckConstraint("ck_ocorrencias_status",
                "status IN ('nova', 'em_andamento', 'finalizada', 'cancelada')");
            t.HasCheckConstraint("ck_ocorrencias_origem",
                "origem IN ('reclamacao_morador', 'reclamacao_funcionario', 'reclamacao_terceiros', 'fora_de_norma')");
            t.HasCheckConstraint("ck_ocorrencias_tipo_local",
                "tipo_local IS NULL OR tipo_local IN ('area_comum','estacionamento','portaria','jardim','salao_festas','hall','corredores','vizinhos','outro')");
            t.HasCheckConstraint("ck_ocorrencias_tipo_ocorrencia",
                "tipo_ocorrencia IS NULL OR tipo_ocorrencia IN ('barulho','pets','garagem','alteracao_fachada','objetos_corredores','objetos_janelas_sacadas','outro')");
            t.HasCheckConstraint("ck_ocorrencias_registrado_xor",
                "(registrado_funcionario_id IS NOT NULL AND registrado_sindico_id IS NULL) OR (registrado_funcionario_id IS NULL AND registrado_sindico_id IS NOT NULL)");
        });

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(o => o.CondominioId).HasColumnName("condominio_id").IsRequired();
        builder.Property(o => o.RegistradoPorFuncionarioId).HasColumnName("registrado_funcionario_id");
        builder.Property(o => o.RegistradoPorSindicoId).HasColumnName("registrado_sindico_id");
        builder.Property(o => o.MoradorId).HasColumnName("morador_id");
        builder.Property(o => o.Origem).HasColumnName("origem").IsRequired();
        builder.Property(o => o.TipoLocal).HasColumnName("tipo_local");
        builder.Property(o => o.BlocoId).HasColumnName("bloco_id");
        builder.Property(o => o.UnidadeId).HasColumnName("unidade_id");
        builder.Property(o => o.TipoOcorrencia).HasColumnName("tipo_ocorrencia");
        builder.Property(o => o.Descricao).HasColumnName("descricao").IsRequired();
        builder.Property(o => o.OcorreuEm).HasColumnName("ocorreu_em").IsRequired();
        builder.Property(o => o.Status).HasColumnName("status").HasDefaultValue("nova");
        builder.Property(o => o.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        builder.Property(o => o.AtualizadoEm).HasColumnName("atualizado_em");

        builder.HasIndex(o => o.CondominioId);
        builder.HasIndex(o => new { o.CondominioId, o.Status });
        builder.HasIndex(o => new { o.CondominioId, o.OcorreuEm });

        builder.HasOne(o => o.Condominio)
            .WithMany(c => c.Ocorrencias)
            .HasForeignKey(o => o.CondominioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.RegistradoPorFuncionario)
            .WithMany()
            .HasForeignKey(o => o.RegistradoPorFuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.RegistradoPorSindico)
            .WithMany()
            .HasForeignKey(o => o.RegistradoPorSindicoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Morador)
            .WithMany(m => m.Ocorrencias)
            .HasForeignKey(o => o.MoradorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.Bloco)
            .WithMany()
            .HasForeignKey(o => o.BlocoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.Unidade)
            .WithMany()
            .HasForeignKey(o => o.UnidadeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(o => o.Midias)
            .WithOne(m => m.Ocorrencia)
            .HasForeignKey(m => m.OcorrenciaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.EmailLogs)
            .WithOne(e => e.Ocorrencia)
            .HasForeignKey(e => e.OcorrenciaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
