using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class OrcamentoManutencaoConfiguration : IEntityTypeConfiguration<OrcamentoManutencao>
{
    public void Configure(EntityTypeBuilder<OrcamentoManutencao> builder)
    {
        builder.ToTable("orcamentos_manutencao", t =>
        {
            t.HasCheckConstraint("ck_orcamento_manutencao_valor", "valor > 0");
            t.HasCheckConstraint(
                "ck_orcamento_manutencao_prazo",
                "prazo_execucao_dias IS NULL OR prazo_execucao_dias > 0");
        });

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(o => o.SolicitacaoManutencaoId).HasColumnName("solicitacao_manutencao_id").IsRequired();
        builder.Property(o => o.FornecedorId).HasColumnName("fornecedor_id");
        builder.Property(o => o.NomeEmpresa).HasColumnName("nome_empresa");
        builder.Property(o => o.NomeContato).HasColumnName("nome_contato");
        builder.Property(o => o.NomeResponsavel).HasColumnName("nome_responsavel");
        builder.Property(o => o.FormaPagamento).HasColumnName("forma_pagamento");
        builder.Property(o => o.DescricaoEscopo).HasColumnName("descricao_escopo");
        builder.Property(o => o.Valor).HasColumnName("valor").HasColumnType("numeric").IsRequired();
        builder.Property(o => o.PrazoExecucaoDias).HasColumnName("prazo_execucao_dias");
        builder.Property(o => o.ValidadeProposta).HasColumnName("validade_proposta");
        builder.Property(o => o.Garantia).HasColumnName("garantia");
        builder.Property(o => o.Observacoes).HasColumnName("observacoes");
        builder.Property(o => o.Selecionada).HasColumnName("selecionada").HasDefaultValue(false);
        builder.Property(o => o.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");

        builder.HasIndex(o => o.SolicitacaoManutencaoId);
        builder.HasIndex(o => o.SolicitacaoManutencaoId)
            .HasFilter("selecionada = true")
            .IsUnique()
            .HasDatabaseName("ix_orcamentos_manutencao_selecionada_unica");
        builder.HasIndex(o => o.FornecedorId);

        builder.HasOne(o => o.SolicitacaoManutencao)
            .WithMany(s => s.Orcamentos)
            .HasForeignKey(o => o.SolicitacaoManutencaoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Fornecedor)
            .WithMany()
            .HasForeignKey(o => o.FornecedorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
