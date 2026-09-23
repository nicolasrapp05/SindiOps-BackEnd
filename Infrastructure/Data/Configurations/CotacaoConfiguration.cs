using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class CotacaoConfiguration : IEntityTypeConfiguration<Cotacao>
{
    public void Configure(EntityTypeBuilder<Cotacao> builder)
    {
        builder.ToTable("cotacoes");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(c => c.SolicitacaoCompraId).HasColumnName("solicitacao_compra_id").IsRequired();
        builder.Property(c => c.FornecedorId).HasColumnName("fornecedor_id");
        builder.Property(c => c.NomeEmpresa).HasColumnName("nome_empresa");
        builder.Property(c => c.NomeContato).HasColumnName("nome_contato");
        builder.Property(c => c.NomeResponsavel).HasColumnName("nome_responsavel");
        builder.Property(c => c.FormaPagamento).HasColumnName("forma_pagamento");
        builder.Property(c => c.Selecionada).HasColumnName("selecionada").HasDefaultValue(false);
        builder.Property(c => c.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");

        builder.HasIndex(c => c.SolicitacaoCompraId);
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
