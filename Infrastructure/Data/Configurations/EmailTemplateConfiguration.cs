using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    public void Configure(EntityTypeBuilder<EmailTemplate> builder)
    {
        builder.ToTable("email_templates", t =>
            t.HasCheckConstraint("ck_email_templates_tipo",
                "tipo IN ('advertencia', 'multa', 'notificacao_ocorrencia', 'comunicado_geral', 'notificacao_manutencao')"));

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.SindicoId).HasColumnName("sindico_id").IsRequired();
        builder.Property(e => e.Nome).HasColumnName("nome").IsRequired();
        builder.Property(e => e.Tipo).HasColumnName("tipo").IsRequired();
        builder.Property(e => e.Assunto).HasColumnName("assunto").IsRequired();
        builder.Property(e => e.Corpo).HasColumnName("corpo").IsRequired();
        builder.Property(e => e.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        builder.Property(e => e.AtualizadoEm).HasColumnName("atualizado_em");

        builder.HasIndex(e => e.SindicoId);

        builder.HasOne(e => e.Sindico)
            .WithMany(s => s.EmailTemplates)
            .HasForeignKey(e => e.SindicoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
