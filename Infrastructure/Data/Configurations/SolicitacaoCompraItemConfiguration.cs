using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class SolicitacaoCompraItemConfiguration : IEntityTypeConfiguration<SolicitacaoCompraItem>
{
    public void Configure(EntityTypeBuilder<SolicitacaoCompraItem> builder)
    {
        builder.ToTable("solicitacao_compra_itens", t =>
        {
            t.HasCheckConstraint("ck_sol_compra_item_categoria",
                "categoria IN ('papelaria', 'mat_construcao', 'mat_limpeza', 'mat_especifico')");
            t.HasCheckConstraint("ck_sol_compra_item_quantidade", "quantidade > 0");
            t.HasCheckConstraint("ck_sol_compra_item_ordem", "ordem >= 0");
        });

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(i => i.SolicitacaoCompraId).HasColumnName("solicitacao_compra_id").IsRequired();
        builder.Property(i => i.Ordem).HasColumnName("ordem").IsRequired();
        builder.Property(i => i.Categoria).HasColumnName("categoria").IsRequired();
        builder.Property(i => i.Descricao).HasColumnName("descricao").IsRequired();
        builder.Property(i => i.Quantidade).HasColumnName("quantidade").HasColumnType("numeric").IsRequired();
        builder.Property(i => i.Unidade).HasColumnName("unidade");
        builder.Property(i => i.EReposicao).HasColumnName("e_reposicao").HasDefaultValue(false);

        builder.HasIndex(i => i.SolicitacaoCompraId);

        builder.HasOne(i => i.SolicitacaoCompra)
            .WithMany(s => s.Itens)
            .HasForeignKey(i => i.SolicitacaoCompraId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
