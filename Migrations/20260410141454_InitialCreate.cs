using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SindiOps.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "sindicos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    nome = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    telefone = table.Column<string>(type: "text", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sindicos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "condominios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    sindico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    endereco_rua = table.Column<string>(type: "text", nullable: true),
                    endereco_numero = table.Column<string>(type: "text", nullable: true),
                    endereco_bairro = table.Column<string>(type: "text", nullable: true),
                    endereco_cidade = table.Column<string>(type: "text", nullable: true),
                    endereco_cep = table.Column<string>(type: "text", nullable: true),
                    data_eleicao = table.Column<DateOnly>(type: "date", nullable: true),
                    vencimento_mandato = table.Column<DateOnly>(type: "date", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_condominios", x => x.id);
                    table.ForeignKey(
                        name: "FK_condominios_sindicos_sindico_id",
                        column: x => x.sindico_id,
                        principalTable: "sindicos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "email_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    sindico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    assunto = table.Column<string>(type: "text", nullable: false),
                    corpo = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_templates", x => x.id);
                    table.CheckConstraint("ck_email_templates_tipo", "tipo IN ('advertencia', 'multa', 'notificacao_ocorrencia', 'comunicado_geral', 'notificacao_manutencao')");
                    table.ForeignKey(
                        name: "FK_email_templates_sindicos_sindico_id",
                        column: x => x.sindico_id,
                        principalTable: "sindicos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fornecedores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    sindico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    cnpj = table.Column<string>(type: "text", nullable: true),
                    endereco_rua = table.Column<string>(type: "text", nullable: true),
                    endereco_numero = table.Column<string>(type: "text", nullable: true),
                    endereco_bairro = table.Column<string>(type: "text", nullable: true),
                    endereco_cidade = table.Column<string>(type: "text", nullable: true),
                    endereco_cep = table.Column<string>(type: "text", nullable: true),
                    telefone = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    instagram = table.Column<string>(type: "text", nullable: true),
                    website = table.Column<string>(type: "text", nullable: true),
                    nome_contato = table.Column<string>(type: "text", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fornecedores", x => x.id);
                    table.ForeignKey(
                        name: "FK_fornecedores_sindicos_sindico_id",
                        column: x => x.sindico_id,
                        principalTable: "sindicos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "funcionarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    sindico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    cargo = table.Column<string>(type: "text", nullable: false),
                    senha_hash = table.Column<string>(type: "text", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_funcionarios", x => x.id);
                    table.CheckConstraint("ck_funcionarios_cargo", "cargo IN ('zelador', 'secretario', 'porteiro', 'outro')");
                    table.ForeignKey(
                        name: "FK_funcionarios_sindicos_sindico_id",
                        column: x => x.sindico_id,
                        principalTable: "sindicos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "blocos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    condominio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blocos", x => x.id);
                    table.ForeignKey(
                        name: "FK_blocos_condominios_condominio_id",
                        column: x => x.condominio_id,
                        principalTable: "condominios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "manutencoes_obrigatorias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    condominio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    data_vencimento = table.Column<DateOnly>(type: "date", nullable: false),
                    ultima_realizacao = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "ok"),
                    observacoes = table.Column<string>(type: "text", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manutencoes_obrigatorias", x => x.id);
                    table.CheckConstraint("ck_manutencoes_status", "status IN ('ok', 'upcoming', 'overdue')");
                    table.CheckConstraint("ck_manutencoes_tipo", "tipo IN ('dedetizacao','para_raios','seguro','limpeza_caixa_agua','caixa_gordura_esgoto','extintores','cvcb','calhas_telhado','ppra','pcmso','pgr')");
                    table.ForeignKey(
                        name: "FK_manutencoes_obrigatorias_condominios_condominio_id",
                        column: x => x.condominio_id,
                        principalTable: "condominios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contratos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    condominio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_servico = table.Column<string>(type: "text", nullable: false),
                    nome_contato = table.Column<string>(type: "text", nullable: true),
                    telefone_contato = table.Column<string>(type: "text", nullable: true),
                    data_inicio = table.Column<DateOnly>(type: "date", nullable: true),
                    data_fim = table.Column<DateOnly>(type: "date", nullable: true),
                    valor_mensal = table.Column<decimal>(type: "numeric", nullable: true),
                    indice_reajuste = table.Column<string>(type: "text", nullable: true),
                    condicoes_renovacao = table.Column<string>(type: "text", nullable: true),
                    condicoes_rescisao = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "active"),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contratos", x => x.id);
                    table.CheckConstraint("ck_contratos_status", "status IN ('active', 'expiring', 'expired', 'cancelled')");
                    table.CheckConstraint("ck_contratos_tipo_servico", "tipo_servico IN ('administradora','garantidora','gas','telefonia','internet','terceirizada','juridico','manutencao_elevador','manutencao_jardim','gestao_residuos','outro')");
                    table.CheckConstraint("ck_contratos_valor_mensal", "valor_mensal IS NULL OR valor_mensal > 0");
                    table.ForeignKey(
                        name: "FK_contratos_condominios_condominio_id",
                        column: x => x.condominio_id,
                        principalTable: "condominios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_contratos_fornecedores_fornecedor_id",
                        column: x => x.fornecedor_id,
                        principalTable: "fornecedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "servicos_fornecedor",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    quantidade = table.Column<decimal>(type: "numeric", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servicos_fornecedor", x => x.id);
                    table.ForeignKey(
                        name: "FK_servicos_fornecedor_fornecedores_fornecedor_id",
                        column: x => x.fornecedor_id,
                        principalTable: "fornecedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "solicitacoes_compra",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    condominio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    solicitado_por = table.Column<Guid>(type: "uuid", nullable: false),
                    categoria = table.Column<string>(type: "text", nullable: false),
                    item = table.Column<string>(type: "text", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric", nullable: false),
                    e_reposicao = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    justificativa = table.Column<string>(type: "text", nullable: true),
                    tipo_aprovacao = table.Column<string>(type: "text", nullable: true),
                    aprovado_por = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "nova"),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solicitacoes_compra", x => x.id);
                    table.CheckConstraint("ck_sol_compra_categoria", "categoria IN ('papelaria', 'mat_construcao', 'mat_limpeza', 'mat_especifico')");
                    table.CheckConstraint("ck_sol_compra_status", "status IN ('nova', 'em_andamento', 'finalizada', 'cancelada')");
                    table.CheckConstraint("ck_sol_compra_tipo_aprovacao", "tipo_aprovacao IS NULL OR tipo_aprovacao IN ('sindico', 'conselho', 'assembleia')");
                    table.ForeignKey(
                        name: "FK_solicitacoes_compra_condominios_condominio_id",
                        column: x => x.condominio_id,
                        principalTable: "condominios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_solicitacoes_compra_funcionarios_aprovado_por",
                        column: x => x.aprovado_por,
                        principalTable: "funcionarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_solicitacoes_compra_funcionarios_solicitado_por",
                        column: x => x.solicitado_por,
                        principalTable: "funcionarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "solicitacoes_manutencao",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    condominio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    solicitado_por = table.Column<Guid>(type: "uuid", nullable: false),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    local = table.Column<string>(type: "text", nullable: true),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    responsavel = table.Column<string>(type: "text", nullable: true),
                    descricao = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "nova"),
                    data_conclusao = table.Column<DateOnly>(type: "date", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solicitacoes_manutencao", x => x.id);
                    table.CheckConstraint("ck_sol_manutencao_responsavel", "responsavel IS NULL OR responsavel IN ('fornecedor', 'zelador')");
                    table.CheckConstraint("ck_sol_manutencao_status", "status IN ('nova', 'em_andamento', 'finalizada', 'cancelada')");
                    table.CheckConstraint("ck_sol_manutencao_tipo", "tipo IN ('obra_civil','pintura','serralheria','eletrica','hidraulica','cameras','portas_portoes','jardim','esgoto','caixa_gordura','outro')");
                    table.ForeignKey(
                        name: "FK_solicitacoes_manutencao_condominios_condominio_id",
                        column: x => x.condominio_id,
                        principalTable: "condominios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_solicitacoes_manutencao_fornecedores_fornecedor_id",
                        column: x => x.fornecedor_id,
                        principalTable: "fornecedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_solicitacoes_manutencao_funcionarios_solicitado_por",
                        column: x => x.solicitado_por,
                        principalTable: "funcionarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "unidades",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    bloco_id = table.Column<Guid>(type: "uuid", nullable: false),
                    condominio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unidades", x => x.id);
                    table.ForeignKey(
                        name: "FK_unidades_blocos_bloco_id",
                        column: x => x.bloco_id,
                        principalTable: "blocos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_unidades_condominios_condominio_id",
                        column: x => x.condominio_id,
                        principalTable: "condominios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cotacoes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    solicitacao_compra_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fornecedor_id = table.Column<Guid>(type: "uuid", nullable: true),
                    nome_empresa = table.Column<string>(type: "text", nullable: true),
                    nome_contato = table.Column<string>(type: "text", nullable: true),
                    nome_responsavel = table.Column<string>(type: "text", nullable: true),
                    valor_unitario = table.Column<decimal>(type: "numeric", nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric", nullable: false),
                    forma_pagamento = table.Column<string>(type: "text", nullable: true),
                    descricao_produto = table.Column<string>(type: "text", nullable: true),
                    quantidade = table.Column<decimal>(type: "numeric", nullable: true),
                    unidade = table.Column<string>(type: "text", nullable: true),
                    selecionada = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cotacoes", x => x.id);
                    table.CheckConstraint("ck_cotacoes_valor_total", "valor_total > 0");
                    table.CheckConstraint("ck_cotacoes_valor_unitario", "valor_unitario > 0");
                    table.ForeignKey(
                        name: "FK_cotacoes_fornecedores_fornecedor_id",
                        column: x => x.fornecedor_id,
                        principalTable: "fornecedores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_cotacoes_solicitacoes_compra_solicitacao_compra_id",
                        column: x => x.solicitacao_compra_id,
                        principalTable: "solicitacoes_compra",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "moradores",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    condominio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bloco_id = table.Column<Guid>(type: "uuid", nullable: false),
                    unidade_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    telefone = table.Column<string>(type: "text", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deletado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_moradores", x => x.id);
                    table.ForeignKey(
                        name: "FK_moradores_blocos_bloco_id",
                        column: x => x.bloco_id,
                        principalTable: "blocos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_moradores_condominios_condominio_id",
                        column: x => x.condominio_id,
                        principalTable: "condominios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_moradores_unidades_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ocorrencias",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    condominio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registrado_por = table.Column<Guid>(type: "uuid", nullable: false),
                    morador_id = table.Column<Guid>(type: "uuid", nullable: true),
                    origem = table.Column<string>(type: "text", nullable: false),
                    tipo_local = table.Column<string>(type: "text", nullable: false),
                    bloco_id = table.Column<Guid>(type: "uuid", nullable: true),
                    unidade_id = table.Column<Guid>(type: "uuid", nullable: true),
                    tipo_ocorrencia = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    ocorreu_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "nova"),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ocorrencias", x => x.id);
                    table.CheckConstraint("ck_ocorrencias_origem", "origem IN ('reclamacao_morador', 'reclamacao_funcionario', 'reclamacao_terceiros', 'fora_de_norma')");
                    table.CheckConstraint("ck_ocorrencias_status", "status IN ('nova', 'em_andamento', 'finalizada', 'cancelada')");
                    table.CheckConstraint("ck_ocorrencias_tipo_local", "tipo_local IS NULL OR tipo_local IN ('area_comum','estacionamento','portaria','jardim','salao_festas','hall','corredores','vizinhos','outro')");
                    table.CheckConstraint("ck_ocorrencias_tipo_ocorrencia", "tipo_ocorrencia IS NULL OR tipo_ocorrencia IN ('barulho','pets','garagem','alteracao_fachada','objetos_corredores','objetos_janelas_sacadas','outro')");
                    table.ForeignKey(
                        name: "FK_ocorrencias_blocos_bloco_id",
                        column: x => x.bloco_id,
                        principalTable: "blocos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ocorrencias_condominios_condominio_id",
                        column: x => x.condominio_id,
                        principalTable: "condominios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ocorrencias_funcionarios_registrado_por",
                        column: x => x.registrado_por,
                        principalTable: "funcionarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ocorrencias_moradores_morador_id",
                        column: x => x.morador_id,
                        principalTable: "moradores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ocorrencias_unidades_unidade_id",
                        column: x => x.unidade_id,
                        principalTable: "unidades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "email_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    sindico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ocorrencia_id = table.Column<Guid>(type: "uuid", nullable: true),
                    morador_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email_destinatario = table.Column<string>(type: "text", nullable: false),
                    assunto = table.Column<string>(type: "text", nullable: false),
                    corpo_resolvido = table.Column<string>(type: "text", nullable: false),
                    valor_multa = table.Column<decimal>(type: "numeric", nullable: true),
                    enviado_por = table.Column<Guid>(type: "uuid", nullable: false),
                    enviado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status_entrega = table.Column<string>(type: "text", nullable: false, defaultValue: "sent"),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_logs", x => x.id);
                    table.CheckConstraint("ck_email_logs_status_entrega", "status_entrega IN ('sent', 'delivered', 'failed')");
                    table.ForeignKey(
                        name: "FK_email_logs_email_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "email_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_email_logs_funcionarios_enviado_por",
                        column: x => x.enviado_por,
                        principalTable: "funcionarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_email_logs_moradores_morador_id",
                        column: x => x.morador_id,
                        principalTable: "moradores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_email_logs_ocorrencias_ocorrencia_id",
                        column: x => x.ocorrencia_id,
                        principalTable: "ocorrencias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_email_logs_sindicos_sindico_id",
                        column: x => x.sindico_id,
                        principalTable: "sindicos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "midias_ocorrencia",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ocorrencia_id = table.Column<Guid>(type: "uuid", nullable: false),
                    url_arquivo = table.Column<string>(type: "text", nullable: false),
                    tipo_arquivo = table.Column<string>(type: "text", nullable: false),
                    enviado_por = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_midias_ocorrencia", x => x.id);
                    table.CheckConstraint("ck_midias_tipo_arquivo", "tipo_arquivo IN ('image', 'video')");
                    table.ForeignKey(
                        name: "FK_midias_ocorrencia_funcionarios_enviado_por",
                        column: x => x.enviado_por,
                        principalTable: "funcionarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_midias_ocorrencia_ocorrencias_ocorrencia_id",
                        column: x => x.ocorrencia_id,
                        principalTable: "ocorrencias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_blocos_condominio_id",
                table: "blocos",
                column: "condominio_id");

            migrationBuilder.CreateIndex(
                name: "IX_condominios_sindico_id",
                table: "condominios",
                column: "sindico_id");

            migrationBuilder.CreateIndex(
                name: "IX_contratos_condominio_id",
                table: "contratos",
                column: "condominio_id");

            migrationBuilder.CreateIndex(
                name: "IX_contratos_condominio_id_status",
                table: "contratos",
                columns: new[] { "condominio_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_contratos_fornecedor_id",
                table: "contratos",
                column: "fornecedor_id");

            migrationBuilder.CreateIndex(
                name: "IX_contratos_status",
                table: "contratos",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_cotacoes_fornecedor_id",
                table: "cotacoes",
                column: "fornecedor_id");

            migrationBuilder.CreateIndex(
                name: "ix_cotacoes_selecionada_unica",
                table: "cotacoes",
                column: "solicitacao_compra_id",
                unique: true,
                filter: "selecionada = true");

            migrationBuilder.CreateIndex(
                name: "IX_email_logs_enviado_por",
                table: "email_logs",
                column: "enviado_por");

            migrationBuilder.CreateIndex(
                name: "IX_email_logs_morador_id",
                table: "email_logs",
                column: "morador_id");

            migrationBuilder.CreateIndex(
                name: "IX_email_logs_ocorrencia_id",
                table: "email_logs",
                column: "ocorrencia_id");

            migrationBuilder.CreateIndex(
                name: "IX_email_logs_sindico_id",
                table: "email_logs",
                column: "sindico_id");

            migrationBuilder.CreateIndex(
                name: "IX_email_logs_status_entrega",
                table: "email_logs",
                column: "status_entrega");

            migrationBuilder.CreateIndex(
                name: "IX_email_logs_template_id",
                table: "email_logs",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_email_templates_sindico_id",
                table: "email_templates",
                column: "sindico_id");

            migrationBuilder.CreateIndex(
                name: "IX_fornecedores_cnpj",
                table: "fornecedores",
                column: "cnpj",
                unique: true,
                filter: "cnpj IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_fornecedores_sindico_id",
                table: "fornecedores",
                column: "sindico_id");

            migrationBuilder.CreateIndex(
                name: "IX_funcionarios_email",
                table: "funcionarios",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_funcionarios_sindico_id",
                table: "funcionarios",
                column: "sindico_id");

            migrationBuilder.CreateIndex(
                name: "IX_manutencoes_obrigatorias_condominio_id",
                table: "manutencoes_obrigatorias",
                column: "condominio_id");

            migrationBuilder.CreateIndex(
                name: "IX_manutencoes_obrigatorias_condominio_id_data_vencimento",
                table: "manutencoes_obrigatorias",
                columns: new[] { "condominio_id", "data_vencimento" });

            migrationBuilder.CreateIndex(
                name: "IX_manutencoes_obrigatorias_status",
                table: "manutencoes_obrigatorias",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_midias_ocorrencia_enviado_por",
                table: "midias_ocorrencia",
                column: "enviado_por");

            migrationBuilder.CreateIndex(
                name: "IX_midias_ocorrencia_ocorrencia_id",
                table: "midias_ocorrencia",
                column: "ocorrencia_id");

            migrationBuilder.CreateIndex(
                name: "IX_moradores_bloco_id",
                table: "moradores",
                column: "bloco_id");

            migrationBuilder.CreateIndex(
                name: "IX_moradores_condominio_id",
                table: "moradores",
                column: "condominio_id");

            migrationBuilder.CreateIndex(
                name: "IX_moradores_unidade_id",
                table: "moradores",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "IX_ocorrencias_bloco_id",
                table: "ocorrencias",
                column: "bloco_id");

            migrationBuilder.CreateIndex(
                name: "IX_ocorrencias_condominio_id",
                table: "ocorrencias",
                column: "condominio_id");

            migrationBuilder.CreateIndex(
                name: "IX_ocorrencias_condominio_id_ocorreu_em",
                table: "ocorrencias",
                columns: new[] { "condominio_id", "ocorreu_em" });

            migrationBuilder.CreateIndex(
                name: "IX_ocorrencias_condominio_id_status",
                table: "ocorrencias",
                columns: new[] { "condominio_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_ocorrencias_morador_id",
                table: "ocorrencias",
                column: "morador_id");

            migrationBuilder.CreateIndex(
                name: "IX_ocorrencias_registrado_por",
                table: "ocorrencias",
                column: "registrado_por");

            migrationBuilder.CreateIndex(
                name: "IX_ocorrencias_unidade_id",
                table: "ocorrencias",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "IX_servicos_fornecedor_fornecedor_id",
                table: "servicos_fornecedor",
                column: "fornecedor_id");

            migrationBuilder.CreateIndex(
                name: "IX_sindicos_email",
                table: "sindicos",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_compra_aprovado_por",
                table: "solicitacoes_compra",
                column: "aprovado_por");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_compra_condominio_id",
                table: "solicitacoes_compra",
                column: "condominio_id");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_compra_condominio_id_status",
                table: "solicitacoes_compra",
                columns: new[] { "condominio_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_compra_solicitado_por",
                table: "solicitacoes_compra",
                column: "solicitado_por");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_manutencao_condominio_id",
                table: "solicitacoes_manutencao",
                column: "condominio_id");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_manutencao_condominio_id_status",
                table: "solicitacoes_manutencao",
                columns: new[] { "condominio_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_manutencao_fornecedor_id",
                table: "solicitacoes_manutencao",
                column: "fornecedor_id");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_manutencao_solicitado_por",
                table: "solicitacoes_manutencao",
                column: "solicitado_por");

            migrationBuilder.CreateIndex(
                name: "IX_unidades_bloco_id",
                table: "unidades",
                column: "bloco_id");

            migrationBuilder.CreateIndex(
                name: "IX_unidades_condominio_id",
                table: "unidades",
                column: "condominio_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contratos");

            migrationBuilder.DropTable(
                name: "cotacoes");

            migrationBuilder.DropTable(
                name: "email_logs");

            migrationBuilder.DropTable(
                name: "manutencoes_obrigatorias");

            migrationBuilder.DropTable(
                name: "midias_ocorrencia");

            migrationBuilder.DropTable(
                name: "servicos_fornecedor");

            migrationBuilder.DropTable(
                name: "solicitacoes_manutencao");

            migrationBuilder.DropTable(
                name: "solicitacoes_compra");

            migrationBuilder.DropTable(
                name: "email_templates");

            migrationBuilder.DropTable(
                name: "ocorrencias");

            migrationBuilder.DropTable(
                name: "fornecedores");

            migrationBuilder.DropTable(
                name: "funcionarios");

            migrationBuilder.DropTable(
                name: "moradores");

            migrationBuilder.DropTable(
                name: "unidades");

            migrationBuilder.DropTable(
                name: "blocos");

            migrationBuilder.DropTable(
                name: "condominios");

            migrationBuilder.DropTable(
                name: "sindicos");
        }
    }
}
