using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class CotacaoConfiguration : IEntityTypeConfiguration<Cotacao>
{
    public void Configure(EntityTypeBuilder<Cotacao> builder)
    {
        builder.ToTable("cotacoes", t =>
        {
            t.HasCheckConstraint("ck_cotacoes_valor_unitario", "valor_unitario > 0");
            t.HasCheckConstraint("ck_cotacoes_valor_total", "valor_total > 0");
        });

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(c => c.SolicitacaoCompraId).HasColumnName("solicitacao_compra_id").IsRequired();
        builder.Property(c => c.FornecedorId).HasColumnName("fornecedor_id");
        builder.Property(c => c.NomeEmpresa).HasColumnName("nome_empresa");
        builder.Property(c => c.NomeContato).HasColumnName("nome_contato");
        builder.Property(c => c.NomeResponsavel).HasColumnName("nome_responsavel");
        builder.Property(c => c.ValorUnitario).HasColumnName("valor_unitario").HasColumnType("numeric").IsRequired();
        builder.Property(c => c.ValorTotal).HasColumnName("valor_total").HasColumnType("numeric").IsRequired();
        builder.Property(c => c.FormaPagamento).HasColumnName("forma_pagamento");
        builder.Property(c => c.DescricaoProduto).HasColumnName("descricao_produto");
        builder.Property(c => c.Quantidade).HasColumnName("quantidade").HasColumnType("numeric");
        builder.Property(c => c.Unidade).HasColumnName("unidade");
        builder.Property(c => c.Selecionada).HasColumnName("selecionada").HasDefaultValue(false);
        builder.Property(c => c.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");

        builder.HasIndex(c => c.SolicitacaoCompraId);

        // índice único parcial: apenas uma cotação selecionada por solicitação
        builder.HasIndex(c => c.SolicitacaoCompraId)
            .HasFilter("selecionada = true")
            .IsUnique()
            .HasDatabaseName("ix_cotacoes_selecionada_unica");

        builder.HasOne(c => c.SolicitacaoCompra)
            .WithMany(s => s.Cotacoes)
            .HasForeignKey(c => c.SolicitacaoCompraId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Fornecedor)
            .WithMany()
            .HasForeignKey(c => c.FornecedorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
