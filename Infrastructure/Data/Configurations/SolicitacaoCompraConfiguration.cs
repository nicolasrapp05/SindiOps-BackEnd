using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class SolicitacaoCompraConfiguration : IEntityTypeConfiguration<SolicitacaoCompra>
{
    public void Configure(EntityTypeBuilder<SolicitacaoCompra> builder)
    {
        builder.ToTable("solicitacoes_compra", t =>
        {
            t.HasCheckConstraint("ck_sol_compra_status",
                "status IN ('nova', 'em_andamento', 'finalizada', 'cancelada')");
            t.HasCheckConstraint("ck_sol_compra_categoria",
                "categoria IN ('papelaria', 'mat_construcao', 'mat_limpeza', 'mat_especifico')");
            t.HasCheckConstraint("ck_sol_compra_tipo_aprovacao",
                "tipo_aprovacao IS NULL OR tipo_aprovacao IN ('sindico', 'conselho', 'assembleia')");
            t.HasCheckConstraint("ck_sol_compra_solicitado_xor",
                "(solicitado_funcionario_id IS NOT NULL AND solicitado_sindico_id IS NULL) OR (solicitado_funcionario_id IS NULL AND solicitado_sindico_id IS NOT NULL)");
        });

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.CondominioId).HasColumnName("condominio_id").IsRequired();
        builder.Property(s => s.SolicitadoPorFuncionarioId).HasColumnName("solicitado_funcionario_id");
        builder.Property(s => s.SolicitadoPorSindicoId).HasColumnName("solicitado_sindico_id");
        builder.Property(s => s.Categoria).HasColumnName("categoria").IsRequired();
        builder.Property(s => s.Item).HasColumnName("item").IsRequired();
        builder.Property(s => s.Quantidade).HasColumnName("quantidade").HasColumnType("numeric").IsRequired();
        builder.Property(s => s.EReposicao).HasColumnName("e_reposicao").HasDefaultValue(false);
        builder.Property(s => s.Justificativa).HasColumnName("justificativa");
        builder.Property(s => s.TipoAprovacao).HasColumnName("tipo_aprovacao");
        builder.Property(s => s.AprovadoPorId).HasColumnName("aprovado_por");
        builder.Property(s => s.Status).HasColumnName("status").HasDefaultValue("nova");
        builder.Property(s => s.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        builder.Property(s => s.AtualizadoEm).HasColumnName("atualizado_em");

        builder.HasIndex(s => s.CondominioId);
        builder.HasIndex(s => new { s.CondominioId, s.Status });

        builder.HasOne(s => s.Condominio)
            .WithMany(c => c.SolicitacoesCompra)
            .HasForeignKey(s => s.CondominioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SolicitadoPorFuncionario)
            .WithMany()
            .HasForeignKey(s => s.SolicitadoPorFuncionarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.SolicitadoPorSindico)
            .WithMany()
            .HasForeignKey(s => s.SolicitadoPorSindicoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.AprovadoPor)
            .WithMany()
            .HasForeignKey(s => s.AprovadoPorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
