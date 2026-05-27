using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class FornecedorConfiguration : IEntityTypeConfiguration<Fornecedor>
{
    public void Configure(EntityTypeBuilder<Fornecedor> builder)
    {
        builder.ToTable("fornecedores");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(f => f.SindicoId).HasColumnName("sindico_id").IsRequired();
        builder.Property(f => f.Nome).HasColumnName("nome").IsRequired();
        builder.Property(f => f.Cnpj).HasColumnName("cnpj");
        builder.Property(f => f.EnderecoRua).HasColumnName("endereco_rua");
        builder.Property(f => f.EnderecoNumero).HasColumnName("endereco_numero");
        builder.Property(f => f.EnderecoBairro).HasColumnName("endereco_bairro");
        builder.Property(f => f.EnderecoCidade).HasColumnName("endereco_cidade");
        builder.Property(f => f.EnderecoCep).HasColumnName("endereco_cep");
        builder.Property(f => f.Telefone).HasColumnName("telefone");
        builder.Property(f => f.Email).HasColumnName("email");
        builder.Property(f => f.Instagram).HasColumnName("instagram");
        builder.Property(f => f.Website).HasColumnName("website");
        builder.Property(f => f.NomeContato).HasColumnName("nome_contato");
        builder.Property(f => f.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        builder.Property(f => f.AtualizadoEm).HasColumnName("atualizado_em");

        builder.HasIndex(f => f.SindicoId);
        builder.HasIndex(f => f.Cnpj).IsUnique().HasFilter("cnpj IS NOT NULL");

        builder.HasOne(f => f.Sindico)
            .WithMany(s => s.Fornecedores)
            .HasForeignKey(f => f.SindicoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
