using Microsoft.EntityFrameworkCore;
using SindiOps.API.Entities;

namespace SindiOps.API.Infrastructure.Data;

public class SindiOpsDbContext : DbContext
{
    public SindiOpsDbContext(DbContextOptions<SindiOpsDbContext> options) : base(options) { }

    protected SindiOpsDbContext(DbContextOptions options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Sindico> Sindicos => Set<Sindico>();
    public DbSet<Funcionario> Funcionarios => Set<Funcionario>();
    public DbSet<FuncionarioCondominio> FuncionarioCondominios => Set<FuncionarioCondominio>();
    public DbSet<Condominio> Condominios => Set<Condominio>();
    public DbSet<Bloco> Blocos => Set<Bloco>();
    public DbSet<Unidade> Unidades => Set<Unidade>();
    public DbSet<Morador> Moradores => Set<Morador>();
    public DbSet<MoradorUnidade> MoradorUnidades => Set<MoradorUnidade>();
    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
    public DbSet<ServicoFornecedor> ServicosFornecedor => Set<ServicoFornecedor>();
    public DbSet<Contrato> Contratos => Set<Contrato>();
    public DbSet<ManutencaoObrigatoria> ManutencoesObrigatorias => Set<ManutencaoObrigatoria>();
    public DbSet<Solicitacao> Solicitacoes => Set<Solicitacao>();
    public DbSet<SolicitacaoManutencao> SolicitacoesManutencao => Set<SolicitacaoManutencao>();
    public DbSet<SolicitacaoCompra> SolicitacoesCompra => Set<SolicitacaoCompra>();
    public DbSet<SolicitacaoCompraItem> SolicitacaoCompraItens => Set<SolicitacaoCompraItem>();
    public DbSet<Cotacao> Cotacoes => Set<Cotacao>();
    public DbSet<CotacaoItem> CotacaoItens => Set<CotacaoItem>();
    public DbSet<OrcamentoManutencao> OrcamentosManutencao => Set<OrcamentoManutencao>();
    public DbSet<Ocorrencia> Ocorrencias => Set<Ocorrencia>();
    public DbSet<MidiaOcorrencia> MidiasOcorrencia => Set<MidiaOcorrencia>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SindiOpsDbContext).Assembly);

        modelBuilder.Entity<MoradorUnidade>()
            .HasQueryFilter(o => o.DeletadoEm == null);
    }
}
