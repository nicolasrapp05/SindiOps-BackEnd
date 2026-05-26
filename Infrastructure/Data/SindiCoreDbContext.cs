using Microsoft.EntityFrameworkCore;
using SindiCore.API.Entities;

namespace SindiCore.API.Infrastructure.Data;

public class SindiCoreDbContext : DbContext
{
    public SindiCoreDbContext(DbContextOptions<SindiCoreDbContext> options) : base(options) { }

    public DbSet<Sindico> Sindicos => Set<Sindico>();
    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();
    public DbSet<Condominio> Condominios => Set<Condominio>();
    public DbSet<Bloco> Blocos => Set<Bloco>();
    public DbSet<Unidade> Unidades => Set<Unidade>();
    public DbSet<Morador> Moradores => Set<Morador>();
    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
    public DbSet<ServicoFornecedor> ServicosFornecedor => Set<ServicoFornecedor>();
    public DbSet<Contrato> Contratos => Set<Contrato>();
    public DbSet<ManutencaoObrigatoria> ManutencoesObrigatorias => Set<ManutencaoObrigatoria>();
    public DbSet<SolicitacaoManutencao> SolicitacoesManutencao => Set<SolicitacaoManutencao>();
    public DbSet<SolicitacaoCompra> SolicitacoesCompra => Set<SolicitacaoCompra>();
    public DbSet<Cotacao> Cotacoes => Set<Cotacao>();
    public DbSet<Ocorrencia> Ocorrencias => Set<Ocorrencia>();
    public DbSet<MidiaOcorrencia> MidiasOcorrencia => Set<MidiaOcorrencia>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SindiCoreDbContext).Assembly);

        modelBuilder.Entity<Morador>()
            .HasQueryFilter(m => m.DeletadoEm == null);
    }
}
