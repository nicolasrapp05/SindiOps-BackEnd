using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class ServicoFornecedorConfiguration : IEntityTypeConfiguration<ServicoFornecedor>
{
    public void Configure(EntityTypeBuilder<ServicoFornecedor> builder)
    {
        builder.ToTable("servicos_fornecedor");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(s => s.FornecedorId).HasColumnName("fornecedor_id").IsRequired();
        builder.Property(s => s.Tipo).HasColumnName("tipo").IsRequired();
        builder.Property(s => s.Descricao).HasColumnName("descricao");
        builder.Property(s => s.Quantidade).HasColumnName("quantidade").HasColumnType("numeric");
        builder.Property(s => s.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");

        builder.HasIndex(s => s.FornecedorId);

        builder.HasOne(s => s.Fornecedor)
            .WithMany(f => f.Servicos)
            .HasForeignKey(s => s.FornecedorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
