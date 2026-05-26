using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SindiCore.API.Entities;

namespace SindiCore.API.Infrastructure.Data.Configurations;

public class ContratoConfiguration : IEntityTypeConfiguration<Contrato>
{
    public void Configure(EntityTypeBuilder<Contrato> builder)
    {
        builder.ToTable("contratos", t =>
        {
            t.HasCheckConstraint("ck_contratos_status",
                "status IN ('active', 'expiring', 'expired', 'cancelled')");
            t.HasCheckConstraint("ck_contratos_tipo_servico",
                "tipo_servico IN ('administradora','garantidora','gas','telefonia','internet','terceirizada','juridico','manutencao_elevador','manutencao_jardim','gestao_residuos','outro')");
            t.HasCheckConstraint("ck_contratos_valor_mensal",
                "valor_mensal IS NULL OR valor_mensal > 0");
        });

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(c => c.CondominioId).HasColumnName("condominio_id").IsRequired();
        builder.Property(c => c.FornecedorId).HasColumnName("fornecedor_id").IsRequired();
        builder.Property(c => c.TipoServico).HasColumnName("tipo_servico").IsRequired();
        builder.Property(c => c.NomeContato).HasColumnName("nome_contato");
        builder.Property(c => c.TelefoneContato).HasColumnName("telefone_contato");
        builder.Property(c => c.DataInicio).HasColumnName("data_inicio");
        builder.Property(c => c.DataFim).HasColumnName("data_fim");
        builder.Property(c => c.ValorMensal).HasColumnName("valor_mensal").HasColumnType("numeric");
        builder.Property(c => c.IndiceReajuste).HasColumnName("indice_reajuste");
        builder.Property(c => c.CondicoesRenovacao).HasColumnName("condicoes_renovacao");
        builder.Property(c => c.CondicoesRescisao).HasColumnName("condicoes_rescisao");
        builder.Property(c => c.Status).HasColumnName("status").HasDefaultValue("active");
        builder.Property(c => c.CriadoEm).HasColumnName("criado_em").HasDefaultValueSql("now()");
        builder.Property(c => c.AtualizadoEm).HasColumnName("atualizado_em");

        builder.HasIndex(c => c.CondominioId);
        builder.HasIndex(c => c.FornecedorId);
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => new { c.CondominioId, c.Status });

        builder.HasOne(c => c.Condominio)
            .WithMany(cond => cond.Contratos)
            .HasForeignKey(c => c.CondominioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Fornecedor)
            .WithMany(f => f.Contratos)
            .HasForeignKey(c => c.FornecedorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
