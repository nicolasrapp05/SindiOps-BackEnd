using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data.Configurations;

public class CondominioConfiguration : IEntityTypeConfiguration<Condominio>
{
    public void Configure(EntityTypeBuilder<Condominio> builder)
    {
        builder.ToTable("condominios");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(c => c.SindicoId).HasColumnName("sindico_id").IsRequired();
        builder.Property(c => c.Nome).HasColumnName("nome").IsRequired();
        builder.Property(c => c.EnderecoRua).HasColumnName("endereco_rua");
        builder.Property(c => c.EnderecoNumero).HasColumnName("endereco_numero");
        builder.Property(c => c.EnderecoBairro).HasColumnName("endereco_bairro");
        builder.Property(c => c.EnderecoCidade).HasColumnName("endereco_cidade");
        builder.Property(c => c.EnderecoCep).HasColumnName("endereco_cep");
        builder.Property(c => c.DataEleicao).HasColumnName("data_eleicao");
        builder.Property(c => c.VencimentoMandato).HasColumnName("vencimento_mandato");
        builder.Property(c => c.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        builder.Property(c => c.AtualizadoEm).HasColumnName("atualizado_em");

        builder.HasIndex(c => c.SindicoId);

        builder.HasOne(c => c.Sindico)
            .WithMany(s => s.Condominios)
            .HasForeignKey(c => c.SindicoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
