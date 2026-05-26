using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiCore.API.Entities;

namespace SindiCore.API.Infrastructure.Data.Configurations;

public class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
{
    public void Configure(EntityTypeBuilder<EmailLog> builder)
    {
        builder.ToTable("email_logs", t =>
            t.HasCheckConstraint("ck_email_logs_status_entrega",
                "status_entrega IN ('sent', 'delivered', 'failed')"));

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(e => e.SindicoId).HasColumnName("sindico_id").IsRequired();
        builder.Property(e => e.TemplateId).HasColumnName("template_id");
        builder.Property(e => e.OcorrenciaId).HasColumnName("ocorrencia_id");
        builder.Property(e => e.MoradorId).HasColumnName("morador_id").IsRequired();
        builder.Property(e => e.EmailDestinatario).HasColumnName("email_destinatario").IsRequired();
        builder.Property(e => e.Assunto).HasColumnName("assunto").IsRequired();
        builder.Property(e => e.CorpoResolvido).HasColumnName("corpo_resolvido").IsRequired();
        builder.Property(e => e.ValorMulta).HasColumnName("valor_multa").HasColumnType("numeric");
        builder.Property(e => e.EnviadoPorId).HasColumnName("enviado_por").IsRequired();
        builder.Property(e => e.EnviadoEm).HasColumnName("enviado_em").IsRequired();
        builder.Property(e => e.StatusEntrega).HasColumnName("status_entrega").HasDefaultValue("sent");
        builder.Property(e => e.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");

        builder.HasIndex(e => e.SindicoId);
        builder.HasIndex(e => e.MoradorId);
        builder.HasIndex(e => e.OcorrenciaId);
        builder.HasIndex(e => e.StatusEntrega);

        builder.HasOne(e => e.Sindico)
            .WithMany(s => s.EmailLogs)
            .HasForeignKey(e => e.SindicoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Template)
            .WithMany(t => t.EmailLogs)
            .HasForeignKey(e => e.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Ocorrencia)
            .WithMany(o => o.EmailLogs)
            .HasForeignKey(e => e.OcorrenciaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Morador)
            .WithMany(m => m.EmailLogs)
            .HasForeignKey(e => e.MoradorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Alinha com o query filter global de Morador (soft delete) — evita aviso EF10622.
        builder.HasQueryFilter(e => e.Morador.DeletadoEm == null);

        builder.HasOne(e => e.EnviadoPor)
            .WithMany()
            .HasForeignKey(e => e.EnviadoPorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
