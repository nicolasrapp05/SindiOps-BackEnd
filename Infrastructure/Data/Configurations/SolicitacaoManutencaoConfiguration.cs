using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class SolicitacaoManutencaoConfiguration : IEntityTypeConfiguration<SolicitacaoManutencao>
{
    public void Configure(EntityTypeBuilder<SolicitacaoManutencao> builder)
    {
        builder.ToTable("solicitacoes_manutencao", t =>
        {
            t.HasCheckConstraint("ck_sol_manutencao_status",
                "status IN ('nova', 'em_andamento', 'finalizada', 'cancelada')");
            t.HasCheckConstraint("ck_sol_manutencao_tipo",
                "tipo IN ('obra_civil','pintura','serralheria','eletrica','hidraulica','cameras','portas_portoes','jardim','esgoto','caixa_gordura','outro')");
            t.HasCheckConstraint("ck_sol_manutencao_responsavel",
                "responsavel IS NULL OR responsavel IN ('fornecedor', 'zelador')");
            t.HasCheckConstraint("ck_sol_manutencao_solicitado_xor",
                "(solicitado_funcionario_id IS NOT NULL AND solicitado_sindico_id IS NULL) OR (solicitado_funcionario_id IS NULL AND solicitado_sindico_id IS NOT NULL)");
        });

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.CondominioId).HasColumnName("condominio_id").IsRequired();
        builder.Property(s => s.SolicitadoPorFuncionarioId).HasColumnName("solicitado_funcionario_id");
        builder.Property(s => s.SolicitadoPorSindicoId).HasColumnName("solicitado_sindico_id");
        builder.Property(s => s.FornecedorId).HasColumnName("fornecedor_id");
        builder.Property(s => s.Local).HasColumnName("local");
        builder.Property(s => s.Tipo).HasColumnName("tipo").IsRequired();
        builder.Property(s => s.Responsavel).HasColumnName("responsavel");
        builder.Property(s => s.Descricao).HasColumnName("descricao");
        builder.Property(s => s.Status).HasColumnName("status").HasDefaultValue("nova");
        builder.Property(s => s.DataConclusao).HasColumnName("data_conclusao");
        builder.Property(s => s.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        builder.Property(s => s.AtualizadoEm).HasColumnName("atualizado_em");

        builder.HasIndex(s => s.CondominioId);
        builder.HasIndex(s => new { s.CondominioId, s.Status });

        builder.HasOne(s => s.Condominio)
            .WithMany(c => c.SolicitacoesManutencao)
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

        builder.HasOne(s => s.Fornecedor)
            .WithMany()
            .HasForeignKey(s => s.FornecedorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
