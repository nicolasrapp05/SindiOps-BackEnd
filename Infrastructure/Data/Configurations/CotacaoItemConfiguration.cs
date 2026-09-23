using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class CotacaoItemConfiguration : IEntityTypeConfiguration<CotacaoItem>
{
    public void Configure(EntityTypeBuilder<CotacaoItem> builder)
    {
        builder.ToTable("cotacao_itens", t =>
        {
            t.HasCheckConstraint("ck_cotacao_itens_valor_unitario", "valor_unitario > 0");
            t.HasCheckConstraint("ck_cotacao_itens_valor_total", "valor_total > 0");
        });

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(i => i.CotacaoId).HasColumnName("cotacao_id").IsRequired();
        builder.Property(i => i.ItemId).HasColumnName("item_id").IsRequired();
        builder.Property(i => i.ValorUnitario).HasColumnName("valor_unitario").HasColumnType("numeric").IsRequired();
        builder.Property(i => i.ValorTotal).HasColumnName("valor_total").HasColumnType("numeric").IsRequired();

        builder.HasIndex(i => i.CotacaoId);
        builder.HasIndex(i => new { i.CotacaoId, i.ItemId }).IsUnique();

        builder.HasOne(i => i.Cotacao)
            .WithMany(c => c.Itens)
            .HasForeignKey(i => i.CotacaoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Item)
            .WithMany(item => item.CotacaoItens)
            .HasForeignKey(i => i.ItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
