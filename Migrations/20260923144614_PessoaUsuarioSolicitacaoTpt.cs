using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SindiOps.API.Migrations
{
    /// <inheritdoc />
    public partial class PessoaUsuarioSolicitacaoTpt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE TABLE _stg_login_pessoa (
                    pessoa_id uuid PRIMARY KEY,
                    usuario_id uuid NOT NULL,
                    nome text NOT NULL,
                    email text NOT NULL,
                    telefone text NULL,
                    cargo text NOT NULL,
                    sindico_id uuid NULL,
                    ativo boolean NOT NULL,
                    criado_em timestamptz NOT NULL
                );

                INSERT INTO _stg_login_pessoa (pessoa_id, usuario_id, nome, email, telefone, cargo, sindico_id, ativo, criado_em)
                SELECT gen_random_uuid(), id, nome, email, telefone, 'sindico', NULL, true, criado_em
                FROM sindicos;

                INSERT INTO _stg_login_pessoa (pessoa_id, usuario_id, nome, email, telefone, cargo, sindico_id, ativo, criado_em)
                SELECT gen_random_uuid(), id, nome, email, NULL, cargo, sindico_id, ativo, criado_em
                FROM funcionarios;

                CREATE TABLE _stg_uc AS
                SELECT funcionario_id AS usuario_id, condominio_id, criado_em
                FROM funcionario_condominios;

                CREATE TABLE _stg_ocorrencia AS
                SELECT id, COALESCE(registrado_funcionario_id, registrado_sindico_id) AS registrado_por_id
                FROM ocorrencias;

                CREATE TABLE _stg_midia AS
                SELECT id, COALESCE(enviado_funcionario_id, enviado_sindico_id) AS enviado_por_id
                FROM midias_ocorrencia;

                CREATE TABLE _stg_email AS
                SELECT id, COALESCE(enviado_funcionario_id, enviado_sindico_id) AS enviado_por_id
                FROM email_logs;

                CREATE TABLE _stg_solicitacao AS
                SELECT id, condominio_id, COALESCE(solicitado_funcionario_id, solicitado_sindico_id) AS solicitado_por_id,
                       'compra'::text AS tipo, status, criado_em, atualizado_em
                FROM solicitacoes_compra
                UNION ALL
                SELECT id, condominio_id, COALESCE(solicitado_funcionario_id, solicitado_sindico_id),
                       'manutencao', status, criado_em, atualizado_em
                FROM solicitacoes_manutencao;

                CREATE TABLE _stg_item AS
                SELECT gen_random_uuid() AS id, id AS solicitacao_compra_id, categoria, item AS descricao, quantidade, e_reposicao
                FROM solicitacoes_compra;

                CREATE TABLE _stg_cotacao_item AS
                SELECT gen_random_uuid() AS id, c.id AS cotacao_id, i.id AS item_id, c.valor_unitario, c.valor_total
                FROM cotacoes c
                JOIN _stg_item i ON i.solicitacao_compra_id = c.solicitacao_compra_id;

                CREATE TABLE _stg_morador AS
                SELECT id AS morador_id, gen_random_uuid() AS pessoa_id, nome, email, telefone, criado_em
                FROM moradores;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_condominios_sindicos_sindico_id",
                table: "condominios");

            migrationBuilder.DropForeignKey(
                name: "FK_email_logs_funcionarios_enviado_funcionario_id",
                table: "email_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_email_logs_sindicos_enviado_sindico_id",
                table: "email_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_email_logs_sindicos_sindico_id",
                table: "email_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_email_templates_sindicos_sindico_id",
                table: "email_templates");

            migrationBuilder.DropForeignKey(
                name: "FK_fornecedores_sindicos_sindico_id",
                table: "fornecedores");

            migrationBuilder.DropForeignKey(
                name: "FK_midias_ocorrencia_funcionarios_enviado_funcionario_id",
                table: "midias_ocorrencia");

            migrationBuilder.DropForeignKey(
                name: "FK_midias_ocorrencia_sindicos_enviado_sindico_id",
                table: "midias_ocorrencia");

            migrationBuilder.DropForeignKey(
                name: "FK_moradores_blocos_bloco_id",
                table: "moradores");

            migrationBuilder.DropForeignKey(
                name: "FK_moradores_condominios_condominio_id",
                table: "moradores");

            migrationBuilder.DropForeignKey(
                name: "FK_ocorrencias_funcionarios_registrado_funcionario_id",
                table: "ocorrencias");

            migrationBuilder.DropForeignKey(
                name: "FK_ocorrencias_sindicos_registrado_sindico_id",
                table: "ocorrencias");

            migrationBuilder.DropForeignKey(
                name: "FK_solicitacoes_compra_condominios_condominio_id",
                table: "solicitacoes_compra");

            migrationBuilder.DropForeignKey(
                name: "FK_solicitacoes_compra_funcionarios_aprovado_por",
                table: "solicitacoes_compra");

            migrationBuilder.DropForeignKey(
                name: "FK_solicitacoes_compra_funcionarios_solicitado_funcionario_id",
                table: "solicitacoes_compra");

            migrationBuilder.DropForeignKey(
                name: "FK_solicitacoes_compra_sindicos_solicitado_sindico_id",
                table: "solicitacoes_compra");

            migrationBuilder.DropForeignKey(
                name: "FK_solicitacoes_manutencao_condominios_condominio_id",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropForeignKey(
                name: "FK_solicitacoes_manutencao_funcionarios_solicitado_funcionario_id",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropForeignKey(
                name: "FK_solicitacoes_manutencao_sindicos_solicitado_sindico_id",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropTable(
                name: "funcionario_condominios");

            migrationBuilder.DropTable(
                name: "funcionarios");

            migrationBuilder.DropTable(
                name: "sindicos");

            migrationBuilder.DropIndex(
                name: "IX_solicitacoes_manutencao_condominio_id",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropIndex(
                name: "IX_solicitacoes_manutencao_condominio_id_status",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropIndex(
                name: "IX_solicitacoes_manutencao_solicitado_funcionario_id",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropIndex(
                name: "IX_solicitacoes_manutencao_solicitado_sindico_id",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropCheckConstraint(
                name: "ck_sol_manutencao_responsavel",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropCheckConstraint(
                name: "ck_sol_manutencao_solicitado_xor",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropCheckConstraint(
                name: "ck_sol_manutencao_status",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropCheckConstraint(
                name: "ck_sol_manutencao_tipo",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropIndex(
                name: "IX_solicitacoes_compra_condominio_id",
                table: "solicitacoes_compra");

            migrationBuilder.DropIndex(
                name: "IX_solicitacoes_compra_condominio_id_status",
                table: "solicitacoes_compra");

            migrationBuilder.DropIndex(
                name: "IX_solicitacoes_compra_solicitado_funcionario_id",
                table: "solicitacoes_compra");

            migrationBuilder.DropIndex(
                name: "IX_solicitacoes_compra_solicitado_sindico_id",
                table: "solicitacoes_compra");

            migrationBuilder.DropCheckConstraint(
                name: "ck_sol_compra_categoria",
                table: "solicitacoes_compra");

            migrationBuilder.DropCheckConstraint(
                name: "ck_sol_compra_solicitado_xor",
                table: "solicitacoes_compra");

            migrationBuilder.DropCheckConstraint(
                name: "ck_sol_compra_status",
                table: "solicitacoes_compra");

            migrationBuilder.DropCheckConstraint(
                name: "ck_sol_compra_tipo_aprovacao",
                table: "solicitacoes_compra");

            migrationBuilder.DropIndex(
                name: "IX_ocorrencias_registrado_funcionario_id",
                table: "ocorrencias");

            migrationBuilder.DropIndex(
                name: "IX_ocorrencias_registrado_sindico_id",
                table: "ocorrencias");

            migrationBuilder.DropCheckConstraint(
                name: "ck_ocorrencias_registrado_xor",
                table: "ocorrencias");

            migrationBuilder.DropIndex(
                name: "IX_moradores_bloco_id",
                table: "moradores");

            migrationBuilder.DropIndex(
                name: "IX_moradores_unidade_id",
                table: "moradores");

            migrationBuilder.DropIndex(
                name: "IX_midias_ocorrencia_enviado_funcionario_id",
                table: "midias_ocorrencia");

            migrationBuilder.DropIndex(
                name: "IX_midias_ocorrencia_enviado_sindico_id",
                table: "midias_ocorrencia");

            migrationBuilder.DropCheckConstraint(
                name: "ck_midias_enviado_xor",
                table: "midias_ocorrencia");

            migrationBuilder.DropIndex(
                name: "IX_email_logs_enviado_funcionario_id",
                table: "email_logs");

            migrationBuilder.DropIndex(
                name: "IX_email_logs_enviado_sindico_id",
                table: "email_logs");

            migrationBuilder.DropCheckConstraint(
                name: "ck_email_logs_enviado_xor",
                table: "email_logs");

            migrationBuilder.DropCheckConstraint(
                name: "ck_cotacoes_valor_total",
                table: "cotacoes");

            migrationBuilder.DropCheckConstraint(
                name: "ck_cotacoes_valor_unitario",
                table: "cotacoes");

            migrationBuilder.DropColumn(
                name: "atualizado_em",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropColumn(
                name: "condominio_id",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropColumn(
                name: "criado_em",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropColumn(
                name: "solicitado_funcionario_id",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropColumn(
                name: "solicitado_sindico_id",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropColumn(
                name: "status",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropColumn(
                name: "atualizado_em",
                table: "solicitacoes_compra");

            migrationBuilder.DropColumn(
                name: "categoria",
                table: "solicitacoes_compra");

            migrationBuilder.DropColumn(
                name: "condominio_id",
                table: "solicitacoes_compra");

            migrationBuilder.DropColumn(
                name: "criado_em",
                table: "solicitacoes_compra");

            migrationBuilder.DropColumn(
                name: "e_reposicao",
                table: "solicitacoes_compra");

            migrationBuilder.DropColumn(
                name: "item",
                table: "solicitacoes_compra");

            migrationBuilder.DropColumn(
                name: "quantidade",
                table: "solicitacoes_compra");

            migrationBuilder.DropColumn(
                name: "solicitado_funcionario_id",
                table: "solicitacoes_compra");

            migrationBuilder.DropColumn(
                name: "solicitado_sindico_id",
                table: "solicitacoes_compra");

            migrationBuilder.DropColumn(
                name: "status",
                table: "solicitacoes_compra");

            migrationBuilder.DropColumn(
                name: "registrado_funcionario_id",
                table: "ocorrencias");

            migrationBuilder.DropColumn(
                name: "registrado_sindico_id",
                table: "ocorrencias");

            migrationBuilder.DropColumn(
                name: "bloco_id",
                table: "moradores");

            migrationBuilder.DropColumn(
                name: "email",
                table: "moradores");

            migrationBuilder.DropColumn(
                name: "telefone",
                table: "moradores");

            migrationBuilder.DropColumn(
                name: "enviado_funcionario_id",
                table: "midias_ocorrencia");

            migrationBuilder.DropColumn(
                name: "enviado_sindico_id",
                table: "midias_ocorrencia");

            migrationBuilder.DropColumn(
                name: "enviado_funcionario_id",
                table: "email_logs");

            migrationBuilder.DropColumn(
                name: "enviado_sindico_id",
                table: "email_logs");

            migrationBuilder.DropColumn(
                name: "descricao_produto",
                table: "cotacoes");

            migrationBuilder.DropColumn(
                name: "quantidade",
                table: "cotacoes");

            migrationBuilder.DropColumn(
                name: "unidade",
                table: "cotacoes");

            migrationBuilder.DropColumn(
                name: "valor_total",
                table: "cotacoes");

            migrationBuilder.DropColumn(
                name: "valor_unitario",
                table: "cotacoes");

            migrationBuilder.RenameColumn(
                name: "aprovado_por",
                table: "solicitacoes_compra",
                newName: "aprovado_por_id");

            migrationBuilder.RenameIndex(
                name: "IX_solicitacoes_compra_aprovado_por",
                table: "solicitacoes_compra",
                newName: "IX_solicitacoes_compra_aprovado_por_id");

            migrationBuilder.RenameColumn(
                name: "nome",
                table: "moradores",
                newName: "papel");

            migrationBuilder.RenameColumn(
                name: "condominio_id",
                table: "moradores",
                newName: "pessoa_id");

            migrationBuilder.RenameIndex(
                name: "IX_moradores_condominio_id",
                table: "moradores",
                newName: "IX_moradores_pessoa_id");

            migrationBuilder.AddColumn<Guid>(
                name: "registrado_por_id",
                table: "ocorrencias",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "enviado_por_id",
                table: "midias_ocorrencia",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "enviado_por_id",
                table: "email_logs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "pessoas",
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
                    table.PrimaryKey("PK_pessoas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "solicitacao_compra_itens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    solicitacao_compra_id = table.Column<Guid>(type: "uuid", nullable: false),
                    categoria = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    quantidade = table.Column<decimal>(type: "numeric", nullable: false),
                    unidade = table.Column<string>(type: "text", nullable: true),
                    e_reposicao = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solicitacao_compra_itens", x => x.id);
                    table.CheckConstraint("ck_sol_compra_item_categoria", "categoria IN ('papelaria', 'mat_construcao', 'mat_limpeza', 'mat_especifico')");
                    table.CheckConstraint("ck_sol_compra_item_quantidade", "quantidade > 0");
                    table.ForeignKey(
                        name: "FK_solicitacao_compra_itens_solicitacoes_compra_solicitacao_co~",
                        column: x => x.solicitacao_compra_id,
                        principalTable: "solicitacoes_compra",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pessoa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sindico_id = table.Column<Guid>(type: "uuid", nullable: true),
                    cargo = table.Column<string>(type: "text", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                    table.CheckConstraint("ck_usuarios_cargo", "cargo IN ('sindico', 'secretario', 'zelador', 'porteiro', 'outro')");
                    table.CheckConstraint("ck_usuarios_sindico_carteira", "(cargo = 'sindico' AND sindico_id IS NULL) OR (cargo <> 'sindico' AND sindico_id IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_usuarios_pessoas_pessoa_id",
                        column: x => x.pessoa_id,
                        principalTable: "pessoas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_usuarios_usuarios_sindico_id",
                        column: x => x.sindico_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cotacao_itens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    cotacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    valor_unitario = table.Column<decimal>(type: "numeric", nullable: false),
                    valor_total = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cotacao_itens", x => x.id);
                    table.CheckConstraint("ck_cotacao_itens_valor_total", "valor_total > 0");
                    table.CheckConstraint("ck_cotacao_itens_valor_unitario", "valor_unitario > 0");
                    table.ForeignKey(
                        name: "FK_cotacao_itens_cotacoes_cotacao_id",
                        column: x => x.cotacao_id,
                        principalTable: "cotacoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cotacao_itens_solicitacao_compra_itens_item_id",
                        column: x => x.item_id,
                        principalTable: "solicitacao_compra_itens",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "solicitacoes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    condominio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    solicitado_por_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "nova"),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_solicitacoes", x => x.id);
                    table.CheckConstraint("ck_solicitacoes_status", "status IN ('nova', 'em_andamento', 'finalizada', 'cancelada')");
                    table.CheckConstraint("ck_solicitacoes_tipo", "tipo IN ('compra', 'manutencao')");
                    table.ForeignKey(
                        name: "FK_solicitacoes_condominios_condominio_id",
                        column: x => x.condominio_id,
                        principalTable: "condominios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_solicitacoes_usuarios_solicitado_por_id",
                        column: x => x.solicitado_por_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario_condominios",
                columns: table => new
                {
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    condominio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_condominios", x => new { x.usuario_id, x.condominio_id });
                    table.ForeignKey(
                        name: "FK_usuario_condominios_condominios_condominio_id",
                        column: x => x.condominio_id,
                        principalTable: "condominios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuario_condominios_usuarios_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO pessoas (id, nome, email, telefone, criado_em)
                SELECT pessoa_id, nome, email, telefone, criado_em
                FROM _stg_login_pessoa;

                INSERT INTO pessoas (id, nome, email, telefone, criado_em)
                SELECT pessoa_id, nome, email, telefone, criado_em
                FROM _stg_morador;

                INSERT INTO usuarios (id, pessoa_id, sindico_id, cargo, ativo, criado_em)
                SELECT usuario_id, pessoa_id, sindico_id, cargo, ativo, criado_em
                FROM _stg_login_pessoa
                WHERE sindico_id IS NULL;

                INSERT INTO usuarios (id, pessoa_id, sindico_id, cargo, ativo, criado_em)
                SELECT usuario_id, pessoa_id, sindico_id, cargo, ativo, criado_em
                FROM _stg_login_pessoa
                WHERE sindico_id IS NOT NULL;

                INSERT INTO usuario_condominios (usuario_id, condominio_id, criado_em)
                SELECT usuario_id, condominio_id, criado_em
                FROM _stg_uc;

                INSERT INTO solicitacoes (id, condominio_id, solicitado_por_id, tipo, status, criado_em, atualizado_em)
                SELECT id, condominio_id, solicitado_por_id, tipo, status, criado_em, atualizado_em
                FROM _stg_solicitacao;

                INSERT INTO solicitacao_compra_itens (id, solicitacao_compra_id, categoria, descricao, quantidade, e_reposicao)
                SELECT id, solicitacao_compra_id, categoria, descricao, quantidade, e_reposicao
                FROM _stg_item;

                INSERT INTO cotacao_itens (id, cotacao_id, item_id, valor_unitario, valor_total)
                SELECT id, cotacao_id, item_id, valor_unitario, valor_total
                FROM _stg_cotacao_item;

                UPDATE ocorrencias o
                SET registrado_por_id = s.registrado_por_id
                FROM _stg_ocorrencia s
                WHERE o.id = s.id;

                UPDATE midias_ocorrencia m
                SET enviado_por_id = s.enviado_por_id
                FROM _stg_midia s
                WHERE m.id = s.id;

                UPDATE email_logs e
                SET enviado_por_id = s.enviado_por_id
                FROM _stg_email s
                WHERE e.id = s.id;

                UPDATE moradores m
                SET pessoa_id = s.pessoa_id,
                    papel = 'ocupante'
                FROM _stg_morador s
                WHERE m.id = s.morador_id;

                ALTER TABLE ocorrencias ALTER COLUMN registrado_por_id DROP DEFAULT;
                ALTER TABLE midias_ocorrencia ALTER COLUMN enviado_por_id DROP DEFAULT;
                ALTER TABLE email_logs ALTER COLUMN enviado_por_id DROP DEFAULT;

                DROP TABLE _stg_login_pessoa;
                DROP TABLE _stg_uc;
                DROP TABLE _stg_ocorrencia;
                DROP TABLE _stg_midia;
                DROP TABLE _stg_email;
                DROP TABLE _stg_solicitacao;
                DROP TABLE _stg_item;
                DROP TABLE _stg_cotacao_item;
                DROP TABLE _stg_morador;
                """);

            migrationBuilder.AddCheckConstraint(
                name: "ck_sol_manutencao_responsavel",
                table: "solicitacoes_manutencao",
                sql: "responsavel IS NULL OR responsavel IN ('fornecedor', 'zelador')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_sol_manutencao_tipo",
                table: "solicitacoes_manutencao",
                sql: "tipo IN ('obra_civil','pintura','serralheria','eletrica','hidraulica','cameras','portas_portoes','jardim','esgoto','caixa_gordura','outro')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_sol_compra_tipo_aprovacao",
                table: "solicitacoes_compra",
                sql: "tipo_aprovacao IS NULL OR tipo_aprovacao IN ('sindico', 'conselho', 'assembleia')");

            migrationBuilder.CreateIndex(
                name: "IX_ocorrencias_registrado_por_id",
                table: "ocorrencias",
                column: "registrado_por_id");

            migrationBuilder.CreateIndex(
                name: "ix_moradores_proprietario_unico",
                table: "moradores",
                column: "unidade_id",
                unique: true,
                filter: "papel = 'proprietario' AND deletado_em IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_moradores_unidade_pessoa",
                table: "moradores",
                columns: new[] { "unidade_id", "pessoa_id" },
                unique: true,
                filter: "deletado_em IS NULL");

            migrationBuilder.AddCheckConstraint(
                name: "ck_moradores_papel",
                table: "moradores",
                sql: "papel IN ('proprietario', 'inquilino', 'ocupante')");

            migrationBuilder.CreateIndex(
                name: "IX_midias_ocorrencia_enviado_por_id",
                table: "midias_ocorrencia",
                column: "enviado_por_id");

            migrationBuilder.CreateIndex(
                name: "IX_email_logs_enviado_por_id",
                table: "email_logs",
                column: "enviado_por_id");

            migrationBuilder.CreateIndex(
                name: "IX_cotacao_itens_cotacao_id",
                table: "cotacao_itens",
                column: "cotacao_id");

            migrationBuilder.CreateIndex(
                name: "IX_cotacao_itens_cotacao_id_item_id",
                table: "cotacao_itens",
                columns: new[] { "cotacao_id", "item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cotacao_itens_item_id",
                table: "cotacao_itens",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacao_compra_itens_solicitacao_compra_id",
                table: "solicitacao_compra_itens",
                column: "solicitacao_compra_id");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_condominio_id",
                table: "solicitacoes",
                column: "condominio_id");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_condominio_id_status",
                table: "solicitacoes",
                columns: new[] { "condominio_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_solicitado_por_id",
                table: "solicitacoes",
                column: "solicitado_por_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_condominios_condominio_id",
                table: "usuario_condominios",
                column: "condominio_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_pessoa_id",
                table: "usuarios",
                column: "pessoa_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_sindico_id",
                table: "usuarios",
                column: "sindico_id");

            migrationBuilder.AddForeignKey(
                name: "FK_condominios_usuarios_sindico_id",
                table: "condominios",
                column: "sindico_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_email_logs_usuarios_enviado_por_id",
                table: "email_logs",
                column: "enviado_por_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_email_logs_usuarios_sindico_id",
                table: "email_logs",
                column: "sindico_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_email_templates_usuarios_sindico_id",
                table: "email_templates",
                column: "sindico_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_fornecedores_usuarios_sindico_id",
                table: "fornecedores",
                column: "sindico_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_midias_ocorrencia_usuarios_enviado_por_id",
                table: "midias_ocorrencia",
                column: "enviado_por_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_moradores_pessoas_pessoa_id",
                table: "moradores",
                column: "pessoa_id",
                principalTable: "pessoas",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ocorrencias_usuarios_registrado_por_id",
                table: "ocorrencias",
                column: "registrado_por_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_solicitacoes_compra_solicitacoes_id",
                table: "solicitacoes_compra",
                column: "id",
                principalTable: "solicitacoes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_solicitacoes_compra_usuarios_aprovado_por_id",
                table: "solicitacoes_compra",
                column: "aprovado_por_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_solicitacoes_manutencao_solicitacoes_id",
                table: "solicitacoes_manutencao",
                column: "id",
                principalTable: "solicitacoes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql(
                """
                CREATE OR REPLACE FUNCTION fn_solicitacao_filha_exclusiva()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                DECLARE
                    tipo_pai text;
                    tem_compra boolean;
                    tem_manutencao boolean;
                BEGIN
                    SELECT tipo INTO tipo_pai FROM solicitacoes WHERE id = NEW.id;
                    IF tipo_pai IS NULL THEN
                        RAISE EXCEPTION 'Solicitação % não existe', NEW.id;
                    END IF;

                    SELECT EXISTS (SELECT 1 FROM solicitacoes_compra WHERE id = NEW.id) INTO tem_compra;
                    SELECT EXISTS (SELECT 1 FROM solicitacoes_manutencao WHERE id = NEW.id) INTO tem_manutencao;

                    IF tem_compra AND tem_manutencao THEN
                        RAISE EXCEPTION 'Solicitação % não pode ser compra e manutenção', NEW.id;
                    END IF;

                    IF TG_TABLE_NAME = 'solicitacoes_compra' AND tipo_pai <> 'compra' THEN
                        RAISE EXCEPTION 'Tipo da solicitação % não é compra', NEW.id;
                    END IF;

                    IF TG_TABLE_NAME = 'solicitacoes_manutencao' AND tipo_pai <> 'manutencao' THEN
                        RAISE EXCEPTION 'Tipo da solicitação % não é manutenção', NEW.id;
                    END IF;

                    RETURN NEW;
                END;
                $$;

                CREATE OR REPLACE FUNCTION fn_solicitacao_tem_filha()
                RETURNS trigger
                LANGUAGE plpgsql
                AS $$
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM solicitacoes_compra WHERE id = NEW.id)
                       AND NOT EXISTS (SELECT 1 FROM solicitacoes_manutencao WHERE id = NEW.id) THEN
                        RAISE EXCEPTION 'Solicitação % sem compra ou manutenção', NEW.id;
                    END IF;
                    RETURN NEW;
                END;
                $$;

                CREATE CONSTRAINT TRIGGER trg_solicitacao_compra_exclusiva
                AFTER INSERT OR UPDATE ON solicitacoes_compra
                DEFERRABLE INITIALLY DEFERRED
                FOR EACH ROW
                EXECUTE FUNCTION fn_solicitacao_filha_exclusiva();

                CREATE CONSTRAINT TRIGGER trg_solicitacao_manutencao_exclusiva
                AFTER INSERT OR UPDATE ON solicitacoes_manutencao
                DEFERRABLE INITIALLY DEFERRED
                FOR EACH ROW
                EXECUTE FUNCTION fn_solicitacao_filha_exclusiva();

                CREATE CONSTRAINT TRIGGER trg_solicitacao_tem_filha
                AFTER INSERT OR UPDATE ON solicitacoes
                DEFERRABLE INITIALLY DEFERRED
                FOR EACH ROW
                EXECUTE FUNCTION fn_solicitacao_tem_filha();
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP TRIGGER IF EXISTS trg_solicitacao_tem_filha ON solicitacoes;
                DROP TRIGGER IF EXISTS trg_solicitacao_compra_exclusiva ON solicitacoes_compra;
                DROP TRIGGER IF EXISTS trg_solicitacao_manutencao_exclusiva ON solicitacoes_manutencao;
                DROP FUNCTION IF EXISTS fn_solicitacao_tem_filha();
                DROP FUNCTION IF EXISTS fn_solicitacao_filha_exclusiva();
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_condominios_usuarios_sindico_id",
                table: "condominios");

            migrationBuilder.DropForeignKey(
                name: "FK_email_logs_usuarios_enviado_por_id",
                table: "email_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_email_logs_usuarios_sindico_id",
                table: "email_logs");

            migrationBuilder.DropForeignKey(
                name: "FK_email_templates_usuarios_sindico_id",
                table: "email_templates");

            migrationBuilder.DropForeignKey(
                name: "FK_fornecedores_usuarios_sindico_id",
                table: "fornecedores");

            migrationBuilder.DropForeignKey(
                name: "FK_midias_ocorrencia_usuarios_enviado_por_id",
                table: "midias_ocorrencia");

            migrationBuilder.DropForeignKey(
                name: "FK_moradores_pessoas_pessoa_id",
                table: "moradores");

            migrationBuilder.DropForeignKey(
                name: "FK_ocorrencias_usuarios_registrado_por_id",
                table: "ocorrencias");

            migrationBuilder.DropForeignKey(
                name: "FK_solicitacoes_compra_solicitacoes_id",
                table: "solicitacoes_compra");

            migrationBuilder.DropForeignKey(
                name: "FK_solicitacoes_compra_usuarios_aprovado_por_id",
                table: "solicitacoes_compra");

            migrationBuilder.DropForeignKey(
                name: "FK_solicitacoes_manutencao_solicitacoes_id",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropTable(
                name: "cotacao_itens");

            migrationBuilder.DropTable(
                name: "solicitacoes");

            migrationBuilder.DropTable(
                name: "usuario_condominios");

            migrationBuilder.DropTable(
                name: "solicitacao_compra_itens");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "pessoas");

            migrationBuilder.DropCheckConstraint(
                name: "ck_sol_manutencao_responsavel",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropCheckConstraint(
                name: "ck_sol_manutencao_tipo",
                table: "solicitacoes_manutencao");

            migrationBuilder.DropCheckConstraint(
                name: "ck_sol_compra_tipo_aprovacao",
                table: "solicitacoes_compra");

            migrationBuilder.DropIndex(
                name: "IX_ocorrencias_registrado_por_id",
                table: "ocorrencias");

            migrationBuilder.DropIndex(
                name: "ix_moradores_proprietario_unico",
                table: "moradores");

            migrationBuilder.DropIndex(
                name: "ix_moradores_unidade_pessoa",
                table: "moradores");

            migrationBuilder.DropCheckConstraint(
                name: "ck_moradores_papel",
                table: "moradores");

            migrationBuilder.DropIndex(
                name: "IX_midias_ocorrencia_enviado_por_id",
                table: "midias_ocorrencia");

            migrationBuilder.DropIndex(
                name: "IX_email_logs_enviado_por_id",
                table: "email_logs");

            migrationBuilder.DropColumn(
                name: "registrado_por_id",
                table: "ocorrencias");

            migrationBuilder.DropColumn(
                name: "enviado_por_id",
                table: "midias_ocorrencia");

            migrationBuilder.DropColumn(
                name: "enviado_por_id",
                table: "email_logs");

            migrationBuilder.RenameColumn(
                name: "aprovado_por_id",
                table: "solicitacoes_compra",
                newName: "aprovado_por");

            migrationBuilder.RenameIndex(
                name: "IX_solicitacoes_compra_aprovado_por_id",
                table: "solicitacoes_compra",
                newName: "IX_solicitacoes_compra_aprovado_por");

            migrationBuilder.RenameColumn(
                name: "pessoa_id",
                table: "moradores",
                newName: "condominio_id");

            migrationBuilder.RenameColumn(
                name: "papel",
                table: "moradores",
                newName: "nome");

            migrationBuilder.RenameIndex(
                name: "IX_moradores_pessoa_id",
                table: "moradores",
                newName: "IX_moradores_condominio_id");

            migrationBuilder.AddColumn<DateTime>(
                name: "atualizado_em",
                table: "solicitacoes_manutencao",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "condominio_id",
                table: "solicitacoes_manutencao",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "criado_em",
                table: "solicitacoes_manutencao",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<Guid>(
                name: "solicitado_funcionario_id",
                table: "solicitacoes_manutencao",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "solicitado_sindico_id",
                table: "solicitacoes_manutencao",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "solicitacoes_manutencao",
                type: "text",
                nullable: false,
                defaultValue: "nova");

            migrationBuilder.AddColumn<DateTime>(
                name: "atualizado_em",
                table: "solicitacoes_compra",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "categoria",
                table: "solicitacoes_compra",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "condominio_id",
                table: "solicitacoes_compra",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "criado_em",
                table: "solicitacoes_compra",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<bool>(
                name: "e_reposicao",
                table: "solicitacoes_compra",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "item",
                table: "solicitacoes_compra",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "quantidade",
                table: "solicitacoes_compra",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "solicitado_funcionario_id",
                table: "solicitacoes_compra",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "solicitado_sindico_id",
                table: "solicitacoes_compra",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "solicitacoes_compra",
                type: "text",
                nullable: false,
                defaultValue: "nova");

            migrationBuilder.AddColumn<Guid>(
                name: "registrado_funcionario_id",
                table: "ocorrencias",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "registrado_sindico_id",
                table: "ocorrencias",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "bloco_id",
                table: "moradores",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "moradores",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "telefone",
                table: "moradores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "enviado_funcionario_id",
                table: "midias_ocorrencia",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "enviado_sindico_id",
                table: "midias_ocorrencia",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "enviado_funcionario_id",
                table: "email_logs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "enviado_sindico_id",
                table: "email_logs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "descricao_produto",
                table: "cotacoes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "quantidade",
                table: "cotacoes",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unidade",
                table: "cotacoes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_total",
                table: "cotacoes",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_unitario",
                table: "cotacoes",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "sindicos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    email = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    telefone = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sindicos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "funcionarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    sindico_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    atualizado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    cargo = table.Column<string>(type: "text", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    email = table.Column<string>(type: "text", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    senha_hash = table.Column<string>(type: "text", nullable: false)
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
                name: "funcionario_condominios",
                columns: table => new
                {
                    funcionario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    condominio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_funcionario_condominios", x => new { x.funcionario_id, x.condominio_id });
                    table.ForeignKey(
                        name: "FK_funcionario_condominios_condominios_condominio_id",
                        column: x => x.condominio_id,
                        principalTable: "condominios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_funcionario_condominios_funcionarios_funcionario_id",
                        column: x => x.funcionario_id,
                        principalTable: "funcionarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_manutencao_condominio_id",
                table: "solicitacoes_manutencao",
                column: "condominio_id");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_manutencao_condominio_id_status",
                table: "solicitacoes_manutencao",
                columns: new[] { "condominio_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_manutencao_solicitado_funcionario_id",
                table: "solicitacoes_manutencao",
                column: "solicitado_funcionario_id");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_manutencao_solicitado_sindico_id",
                table: "solicitacoes_manutencao",
                column: "solicitado_sindico_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_sol_manutencao_responsavel",
                table: "solicitacoes_manutencao",
                sql: "responsavel IS NULL OR responsavel IN ('fornecedor', 'zelador')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_sol_manutencao_solicitado_xor",
                table: "solicitacoes_manutencao",
                sql: "(solicitado_funcionario_id IS NOT NULL AND solicitado_sindico_id IS NULL) OR (solicitado_funcionario_id IS NULL AND solicitado_sindico_id IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_sol_manutencao_status",
                table: "solicitacoes_manutencao",
                sql: "status IN ('nova', 'em_andamento', 'finalizada', 'cancelada')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_sol_manutencao_tipo",
                table: "solicitacoes_manutencao",
                sql: "tipo IN ('obra_civil','pintura','serralheria','eletrica','hidraulica','cameras','portas_portoes','jardim','esgoto','caixa_gordura','outro')");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_compra_condominio_id",
                table: "solicitacoes_compra",
                column: "condominio_id");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_compra_condominio_id_status",
                table: "solicitacoes_compra",
                columns: new[] { "condominio_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_compra_solicitado_funcionario_id",
                table: "solicitacoes_compra",
                column: "solicitado_funcionario_id");

            migrationBuilder.CreateIndex(
                name: "IX_solicitacoes_compra_solicitado_sindico_id",
                table: "solicitacoes_compra",
                column: "solicitado_sindico_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_sol_compra_categoria",
                table: "solicitacoes_compra",
                sql: "categoria IN ('papelaria', 'mat_construcao', 'mat_limpeza', 'mat_especifico')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_sol_compra_solicitado_xor",
                table: "solicitacoes_compra",
                sql: "(solicitado_funcionario_id IS NOT NULL AND solicitado_sindico_id IS NULL) OR (solicitado_funcionario_id IS NULL AND solicitado_sindico_id IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_sol_compra_status",
                table: "solicitacoes_compra",
                sql: "status IN ('nova', 'em_andamento', 'finalizada', 'cancelada')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_sol_compra_tipo_aprovacao",
                table: "solicitacoes_compra",
                sql: "tipo_aprovacao IS NULL OR tipo_aprovacao IN ('sindico', 'conselho', 'assembleia')");

            migrationBuilder.CreateIndex(
                name: "IX_ocorrencias_registrado_funcionario_id",
                table: "ocorrencias",
                column: "registrado_funcionario_id");

            migrationBuilder.CreateIndex(
                name: "IX_ocorrencias_registrado_sindico_id",
                table: "ocorrencias",
                column: "registrado_sindico_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_ocorrencias_registrado_xor",
                table: "ocorrencias",
                sql: "(registrado_funcionario_id IS NOT NULL AND registrado_sindico_id IS NULL) OR (registrado_funcionario_id IS NULL AND registrado_sindico_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_moradores_bloco_id",
                table: "moradores",
                column: "bloco_id");

            migrationBuilder.CreateIndex(
                name: "IX_moradores_unidade_id",
                table: "moradores",
                column: "unidade_id");

            migrationBuilder.CreateIndex(
                name: "IX_midias_ocorrencia_enviado_funcionario_id",
                table: "midias_ocorrencia",
                column: "enviado_funcionario_id");

            migrationBuilder.CreateIndex(
                name: "IX_midias_ocorrencia_enviado_sindico_id",
                table: "midias_ocorrencia",
                column: "enviado_sindico_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_midias_enviado_xor",
                table: "midias_ocorrencia",
                sql: "(enviado_funcionario_id IS NOT NULL AND enviado_sindico_id IS NULL) OR (enviado_funcionario_id IS NULL AND enviado_sindico_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_email_logs_enviado_funcionario_id",
                table: "email_logs",
                column: "enviado_funcionario_id");

            migrationBuilder.CreateIndex(
                name: "IX_email_logs_enviado_sindico_id",
                table: "email_logs",
                column: "enviado_sindico_id");

            migrationBuilder.AddCheckConstraint(
                name: "ck_email_logs_enviado_xor",
                table: "email_logs",
                sql: "(enviado_funcionario_id IS NOT NULL AND enviado_sindico_id IS NULL) OR (enviado_funcionario_id IS NULL AND enviado_sindico_id IS NOT NULL)");

            migrationBuilder.AddCheckConstraint(
                name: "ck_cotacoes_valor_total",
                table: "cotacoes",
                sql: "valor_total > 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_cotacoes_valor_unitario",
                table: "cotacoes",
                sql: "valor_unitario > 0");

            migrationBuilder.CreateIndex(
                name: "IX_funcionario_condominios_condominio_id",
                table: "funcionario_condominios",
                column: "condominio_id");

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
                name: "IX_sindicos_email",
                table: "sindicos",
                column: "email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_condominios_sindicos_sindico_id",
                table: "condominios",
                column: "sindico_id",
                principalTable: "sindicos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_email_logs_funcionarios_enviado_funcionario_id",
                table: "email_logs",
                column: "enviado_funcionario_id",
                principalTable: "funcionarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_email_logs_sindicos_enviado_sindico_id",
                table: "email_logs",
                column: "enviado_sindico_id",
                principalTable: "sindicos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_email_logs_sindicos_sindico_id",
                table: "email_logs",
                column: "sindico_id",
                principalTable: "sindicos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_email_templates_sindicos_sindico_id",
                table: "email_templates",
                column: "sindico_id",
                principalTable: "sindicos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_fornecedores_sindicos_sindico_id",
                table: "fornecedores",
                column: "sindico_id",
                principalTable: "sindicos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_midias_ocorrencia_funcionarios_enviado_funcionario_id",
                table: "midias_ocorrencia",
                column: "enviado_funcionario_id",
                principalTable: "funcionarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_midias_ocorrencia_sindicos_enviado_sindico_id",
                table: "midias_ocorrencia",
                column: "enviado_sindico_id",
                principalTable: "sindicos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_moradores_blocos_bloco_id",
                table: "moradores",
                column: "bloco_id",
                principalTable: "blocos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_moradores_condominios_condominio_id",
                table: "moradores",
                column: "condominio_id",
                principalTable: "condominios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ocorrencias_funcionarios_registrado_funcionario_id",
                table: "ocorrencias",
                column: "registrado_funcionario_id",
                principalTable: "funcionarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ocorrencias_sindicos_registrado_sindico_id",
                table: "ocorrencias",
                column: "registrado_sindico_id",
                principalTable: "sindicos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_solicitacoes_compra_condominios_condominio_id",
                table: "solicitacoes_compra",
                column: "condominio_id",
                principalTable: "condominios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_solicitacoes_compra_funcionarios_aprovado_por",
                table: "solicitacoes_compra",
                column: "aprovado_por",
                principalTable: "funcionarios",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_solicitacoes_compra_funcionarios_solicitado_funcionario_id",
                table: "solicitacoes_compra",
                column: "solicitado_funcionario_id",
                principalTable: "funcionarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_solicitacoes_compra_sindicos_solicitado_sindico_id",
                table: "solicitacoes_compra",
                column: "solicitado_sindico_id",
                principalTable: "sindicos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_solicitacoes_manutencao_condominios_condominio_id",
                table: "solicitacoes_manutencao",
                column: "condominio_id",
                principalTable: "condominios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_solicitacoes_manutencao_funcionarios_solicitado_funcionario_id",
                table: "solicitacoes_manutencao",
                column: "solicitado_funcionario_id",
                principalTable: "funcionarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_solicitacoes_manutencao_sindicos_solicitado_sindico_id",
                table: "solicitacoes_manutencao",
                column: "solicitado_sindico_id",
                principalTable: "sindicos",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
