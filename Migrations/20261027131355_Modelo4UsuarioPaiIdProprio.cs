using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SindiOps.API.Migrations
{
    /// <inheritdoc />
    public partial class Modelo4UsuarioPaiIdProprio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Copia os dados do modelo pessoa/cargo para usuários com papéis próprios
            // antes de remover tabelas e colunas. IDs de login e de cada ocupação
            // de morador são preservados. O id do síndico e do funcionário migrados
            // reutiliza o id do usuário, que já era a chave pública desses papéis.
            migrationBuilder.Sql(
                """
                DO $mig$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM usuarios u
                        WHERE NOT EXISTS (SELECT 1 FROM pessoas p WHERE p.id = u.pessoa_id)
                    ) THEN
                        RAISE EXCEPTION 'Usuário sem pessoa vinculada';
                    END IF;

                    IF EXISTS (
                        SELECT 1
                        FROM usuarios u
                        WHERE u.cargo <> 'sindico'
                          AND NOT EXISTS (
                              SELECT 1
                              FROM usuarios emp
                              WHERE emp.id = u.sindico_id
                                AND emp.cargo = 'sindico'
                          )
                    ) THEN
                        RAISE EXCEPTION 'Funcionário sem síndico válido na carteira';
                    END IF;

                    IF EXISTS (
                        SELECT 1 FROM condominios c
                        WHERE NOT EXISTS (
                            SELECT 1 FROM usuarios u
                            WHERE u.id = c.sindico_id AND u.cargo = 'sindico'
                        )
                    ) OR EXISTS (
                        SELECT 1 FROM fornecedores f
                        WHERE NOT EXISTS (
                            SELECT 1 FROM usuarios u
                            WHERE u.id = f.sindico_id AND u.cargo = 'sindico'
                        )
                    ) OR EXISTS (
                        SELECT 1 FROM email_templates t
                        WHERE NOT EXISTS (
                            SELECT 1 FROM usuarios u
                            WHERE u.id = t.sindico_id AND u.cargo = 'sindico'
                        )
                    ) OR EXISTS (
                        SELECT 1 FROM email_logs e
                        WHERE NOT EXISTS (
                            SELECT 1 FROM usuarios u
                            WHERE u.id = e.sindico_id AND u.cargo = 'sindico'
                        )
                    ) THEN
                        RAISE EXCEPTION 'Registro de carteira aponta para um síndico inválido';
                    END IF;
                END
                $mig$;

                ALTER TABLE usuarios ADD COLUMN nome text;
                ALTER TABLE usuarios ADD COLUMN email text;
                ALTER TABLE usuarios ADD COLUMN telefone text;

                UPDATE usuarios u
                SET nome = p.nome,
                    email = p.email,
                    telefone = p.telefone
                FROM pessoas p
                WHERE p.id = u.pessoa_id;

                DO $mig$
                BEGIN
                    IF EXISTS (SELECT 1 FROM usuarios WHERE nome IS NULL OR email IS NULL) THEN
                        RAISE EXCEPTION 'Usuário sem nome ou email para migrar';
                    END IF;
                END
                $mig$;

                CREATE TEMP TABLE migracao_contagem AS
                SELECT
                    (SELECT COUNT(*) FROM moradores) AS moradores_antes,
                    (SELECT COUNT(*) FROM pessoas) AS pessoas_antes,
                    (SELECT COUNT(*) FROM usuarios WHERE cargo = 'sindico') AS sindicos_antes,
                    (SELECT COUNT(*) FROM usuarios WHERE cargo <> 'sindico') AS funcionarios_antes,
                    (SELECT COUNT(*)
                     FROM usuario_condominios uc
                     WHERE EXISTS (
                         SELECT 1 FROM usuarios u
                         WHERE u.id = uc.usuario_id AND u.cargo <> 'sindico'
                     )) AS acessos_antes;

                CREATE TABLE sindicos (
                    id uuid NOT NULL DEFAULT gen_random_uuid(),
                    usuario_id uuid NOT NULL,
                    criado_em timestamp with time zone NOT NULL DEFAULT now(),
                    atualizado_em timestamp with time zone NULL,
                    CONSTRAINT "PK_sindicos" PRIMARY KEY (id),
                    CONSTRAINT "FK_sindicos_usuarios_usuario_id"
                        FOREIGN KEY (usuario_id) REFERENCES usuarios (id) ON DELETE RESTRICT
                );

                INSERT INTO sindicos (id, usuario_id, criado_em, atualizado_em)
                SELECT u.id, u.id, u.criado_em, u.atualizado_em
                FROM usuarios u
                WHERE u.cargo = 'sindico';

                CREATE UNIQUE INDEX "IX_sindicos_usuario_id" ON sindicos (usuario_id);

                CREATE TABLE funcionarios (
                    id uuid NOT NULL DEFAULT gen_random_uuid(),
                    usuario_id uuid NOT NULL,
                    sindico_id uuid NOT NULL,
                    cargo text NOT NULL,
                    criado_em timestamp with time zone NOT NULL DEFAULT now(),
                    atualizado_em timestamp with time zone NULL,
                    CONSTRAINT "PK_funcionarios" PRIMARY KEY (id),
                    CONSTRAINT ck_funcionarios_cargo
                        CHECK (cargo IN ('secretario', 'zelador', 'porteiro', 'outro')),
                    CONSTRAINT "FK_funcionarios_sindicos_sindico_id"
                        FOREIGN KEY (sindico_id) REFERENCES sindicos (id) ON DELETE RESTRICT,
                    CONSTRAINT "FK_funcionarios_usuarios_usuario_id"
                        FOREIGN KEY (usuario_id) REFERENCES usuarios (id) ON DELETE RESTRICT
                );

                INSERT INTO funcionarios (id, usuario_id, sindico_id, cargo, criado_em, atualizado_em)
                SELECT u.id, u.id, u.sindico_id, u.cargo, u.criado_em, u.atualizado_em
                FROM usuarios u
                WHERE u.cargo <> 'sindico';

                CREATE UNIQUE INDEX "IX_funcionarios_usuario_id" ON funcionarios (usuario_id);
                CREATE INDEX "IX_funcionarios_sindico_id" ON funcionarios (sindico_id);

                CREATE TABLE funcionario_condominios (
                    funcionario_id uuid NOT NULL,
                    condominio_id uuid NOT NULL,
                    criado_em timestamp with time zone NOT NULL DEFAULT now(),
                    CONSTRAINT "PK_funcionario_condominios" PRIMARY KEY (funcionario_id, condominio_id),
                    CONSTRAINT "FK_funcionario_condominios_condominios_condominio_id"
                        FOREIGN KEY (condominio_id) REFERENCES condominios (id) ON DELETE CASCADE,
                    CONSTRAINT "FK_funcionario_condominios_funcionarios_funcionario_id"
                        FOREIGN KEY (funcionario_id) REFERENCES funcionarios (id) ON DELETE CASCADE
                );

                INSERT INTO funcionario_condominios (funcionario_id, condominio_id, criado_em)
                SELECT f.id, uc.condominio_id, uc.criado_em
                FROM usuario_condominios uc
                JOIN funcionarios f ON f.usuario_id = uc.usuario_id;

                CREATE INDEX "IX_funcionario_condominios_condominio_id"
                    ON funcionario_condominios (condominio_id);

                ALTER TABLE condominios DROP CONSTRAINT "FK_condominios_usuarios_sindico_id";
                ALTER TABLE email_logs DROP CONSTRAINT "FK_email_logs_usuarios_sindico_id";
                ALTER TABLE email_templates DROP CONSTRAINT "FK_email_templates_usuarios_sindico_id";
                ALTER TABLE fornecedores DROP CONSTRAINT "FK_fornecedores_usuarios_sindico_id";

                UPDATE condominios c
                SET sindico_id = s.id
                FROM sindicos s
                WHERE s.usuario_id = c.sindico_id;

                UPDATE email_logs e
                SET sindico_id = s.id
                FROM sindicos s
                WHERE s.usuario_id = e.sindico_id;

                UPDATE email_templates t
                SET sindico_id = s.id
                FROM sindicos s
                WHERE s.usuario_id = t.sindico_id;

                UPDATE fornecedores f
                SET sindico_id = s.id
                FROM sindicos s
                WHERE s.usuario_id = f.sindico_id;

                ALTER TABLE condominios
                    ADD CONSTRAINT "FK_condominios_sindicos_sindico_id"
                    FOREIGN KEY (sindico_id) REFERENCES sindicos (id) ON DELETE RESTRICT;
                ALTER TABLE email_logs
                    ADD CONSTRAINT "FK_email_logs_sindicos_sindico_id"
                    FOREIGN KEY (sindico_id) REFERENCES sindicos (id) ON DELETE RESTRICT;
                ALTER TABLE email_templates
                    ADD CONSTRAINT "FK_email_templates_sindicos_sindico_id"
                    FOREIGN KEY (sindico_id) REFERENCES sindicos (id) ON DELETE RESTRICT;
                ALTER TABLE fornecedores
                    ADD CONSTRAINT "FK_fornecedores_sindicos_sindico_id"
                    FOREIGN KEY (sindico_id) REFERENCES sindicos (id) ON DELETE RESTRICT;

                ALTER TABLE usuarios DROP CONSTRAINT ck_usuarios_cargo;
                ALTER TABLE usuarios DROP CONSTRAINT ck_usuarios_sindico_carteira;
                ALTER TABLE usuarios DROP CONSTRAINT "FK_usuarios_usuarios_sindico_id";
                ALTER TABLE usuarios DROP CONSTRAINT "FK_usuarios_pessoas_pessoa_id";
                ALTER TABLE usuarios ALTER COLUMN cargo DROP NOT NULL;
                ALTER TABLE usuarios ALTER COLUMN pessoa_id DROP NOT NULL;

                CREATE TEMP TABLE pessoa_usuario_map (
                    pessoa_id uuid PRIMARY KEY,
                    usuario_id uuid NOT NULL
                );

                INSERT INTO pessoa_usuario_map (pessoa_id, usuario_id)
                SELECT pessoa_id, id
                FROM usuarios
                WHERE pessoa_id IS NOT NULL;

                CREATE TEMP TABLE novos_usuarios AS
                SELECT
                    p.id AS pessoa_id,
                    CASE
                        WHEN EXISTS (SELECT 1 FROM usuarios u WHERE u.id = p.id) THEN gen_random_uuid()
                        ELSE p.id
                    END AS usuario_id,
                    p.nome,
                    p.email,
                    p.telefone,
                    p.criado_em,
                    p.atualizado_em
                FROM pessoas p
                WHERE NOT EXISTS (
                    SELECT 1 FROM pessoa_usuario_map m WHERE m.pessoa_id = p.id
                );

                INSERT INTO usuarios (id, nome, email, telefone, ativo, criado_em, atualizado_em)
                SELECT usuario_id, nome, email, telefone, true, criado_em, atualizado_em
                FROM novos_usuarios;

                INSERT INTO pessoa_usuario_map (pessoa_id, usuario_id)
                SELECT pessoa_id, usuario_id
                FROM novos_usuarios;

                ALTER TABLE moradores ADD COLUMN usuario_id uuid;

                UPDATE moradores m
                SET usuario_id = map.usuario_id
                FROM pessoa_usuario_map map
                WHERE map.pessoa_id = m.pessoa_id;

                DO $mig$
                BEGIN
                    IF EXISTS (SELECT 1 FROM moradores WHERE usuario_id IS NULL) THEN
                        RAISE EXCEPTION 'Morador sem usuário correspondente';
                    END IF;

                    IF EXISTS (
                        SELECT 1
                        FROM moradores
                        WHERE deletado_em IS NULL
                        GROUP BY usuario_id, unidade_id
                        HAVING COUNT(*) > 1
                    ) THEN
                        RAISE EXCEPTION 'Há moradores ativos duplicados na mesma unidade para a mesma pessoa';
                    END IF;
                END
                $mig$;

                CREATE TEMP TABLE morador_canon AS
                SELECT DISTINCT ON (usuario_id)
                    id AS morador_id,
                    usuario_id
                FROM moradores
                ORDER BY usuario_id, (deletado_em IS NULL) DESC, criado_em ASC, id ASC;

                CREATE TABLE morador_unidades (
                    id uuid NOT NULL DEFAULT gen_random_uuid(),
                    morador_id uuid NOT NULL,
                    unidade_id uuid NOT NULL,
                    papel text NOT NULL,
                    criado_em timestamp with time zone NOT NULL DEFAULT now(),
                    atualizado_em timestamp with time zone NULL,
                    deletado_em timestamp with time zone NULL,
                    CONSTRAINT "PK_morador_unidades" PRIMARY KEY (id),
                    CONSTRAINT ck_morador_unidades_papel
                        CHECK (papel IN ('proprietario', 'inquilino', 'ocupante')),
                    CONSTRAINT "FK_morador_unidades_moradores_morador_id"
                        FOREIGN KEY (morador_id) REFERENCES moradores (id) ON DELETE CASCADE,
                    CONSTRAINT "FK_morador_unidades_unidades_unidade_id"
                        FOREIGN KEY (unidade_id) REFERENCES unidades (id) ON DELETE RESTRICT
                );

                INSERT INTO morador_unidades (
                    id, morador_id, unidade_id, papel, criado_em, atualizado_em, deletado_em)
                SELECT m.id, c.morador_id, m.unidade_id, m.papel, m.criado_em, m.atualizado_em, m.deletado_em
                FROM moradores m
                JOIN morador_canon c ON c.usuario_id = m.usuario_id;

                CREATE INDEX "IX_morador_unidades_morador_id" ON morador_unidades (morador_id);
                CREATE UNIQUE INDEX ix_morador_unidades_proprietario_unico
                    ON morador_unidades (unidade_id)
                    WHERE papel = 'proprietario' AND deletado_em IS NULL;
                CREATE UNIQUE INDEX ix_morador_unidades_unidade_morador
                    ON morador_unidades (unidade_id, morador_id)
                    WHERE deletado_em IS NULL;

                UPDATE email_logs el
                SET morador_id = c.morador_id
                FROM moradores m
                JOIN morador_canon c ON c.usuario_id = m.usuario_id
                WHERE el.morador_id = m.id
                  AND el.morador_id <> c.morador_id;

                UPDATE ocorrencias o
                SET morador_id = c.morador_id
                FROM moradores m
                JOIN morador_canon c ON c.usuario_id = m.usuario_id
                WHERE o.morador_id = m.id
                  AND o.morador_id IS DISTINCT FROM c.morador_id;

                DELETE FROM moradores m
                USING morador_canon c
                WHERE m.usuario_id = c.usuario_id
                  AND m.id <> c.morador_id;

                DROP INDEX "IX_moradores_pessoa_id";
                DROP INDEX ix_moradores_proprietario_unico;
                DROP INDEX ix_moradores_unidade_pessoa;
                ALTER TABLE moradores DROP CONSTRAINT ck_moradores_papel;
                ALTER TABLE moradores DROP CONSTRAINT "FK_moradores_pessoas_pessoa_id";
                ALTER TABLE moradores DROP CONSTRAINT "FK_moradores_unidades_unidade_id";
                ALTER TABLE moradores DROP COLUMN pessoa_id;
                ALTER TABLE moradores DROP COLUMN unidade_id;
                ALTER TABLE moradores DROP COLUMN papel;
                ALTER TABLE moradores DROP COLUMN deletado_em;

                ALTER TABLE moradores ALTER COLUMN usuario_id SET NOT NULL;
                CREATE UNIQUE INDEX "IX_moradores_usuario_id" ON moradores (usuario_id);
                ALTER TABLE moradores
                    ADD CONSTRAINT "FK_moradores_usuarios_usuario_id"
                    FOREIGN KEY (usuario_id) REFERENCES usuarios (id) ON DELETE RESTRICT;

                DROP INDEX "IX_usuarios_pessoa_id";
                DROP INDEX "IX_usuarios_sindico_id";
                ALTER TABLE usuarios DROP COLUMN pessoa_id;
                ALTER TABLE usuarios DROP COLUMN cargo;
                ALTER TABLE usuarios DROP COLUMN sindico_id;
                ALTER TABLE usuarios ALTER COLUMN nome SET NOT NULL;
                ALTER TABLE usuarios ALTER COLUMN email SET NOT NULL;
                CREATE INDEX "IX_usuarios_email" ON usuarios (email);

                DO $mig$
                BEGIN
                    IF (SELECT COUNT(*) FROM morador_unidades) <> (SELECT moradores_antes FROM migracao_contagem) THEN
                        RAISE EXCEPTION 'A migração perdeu ocupações de moradores';
                    END IF;

                    IF (SELECT COUNT(*) FROM usuarios) <> (SELECT pessoas_antes FROM migracao_contagem) THEN
                        RAISE EXCEPTION 'A quantidade de usuários não corresponde às pessoas migradas';
                    END IF;

                    IF (SELECT COUNT(*) FROM sindicos) <> (SELECT sindicos_antes FROM migracao_contagem) THEN
                        RAISE EXCEPTION 'A migração perdeu síndicos';
                    END IF;

                    IF (SELECT COUNT(*) FROM funcionarios) <> (SELECT funcionarios_antes FROM migracao_contagem) THEN
                        RAISE EXCEPTION 'A migração perdeu funcionários';
                    END IF;

                    IF (SELECT COUNT(*) FROM funcionario_condominios) <> (SELECT acessos_antes FROM migracao_contagem) THEN
                        RAISE EXCEPTION 'A migração perdeu acessos de funcionários a condomínios';
                    END IF;

                    IF EXISTS (
                        SELECT usuario_id FROM moradores GROUP BY usuario_id HAVING COUNT(*) > 1
                    ) THEN
                        RAISE EXCEPTION 'Mais de um morador por usuário após a migração';
                    END IF;
                END
                $mig$;

                DROP TABLE usuario_condominios;
                DROP TABLE pessoas;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reconstrói pessoas, cargo e uma linha de morador por ocupação.
            // O id original de pessoas de quem já tinha login não é recuperável.
            // Ocorrências e emails agrupados na mesma pessoa voltam a apontar
            // para a ocupação canônica, não para cada unidade de origem.
            migrationBuilder.Sql(
                """
                DO $mig$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM moradores m
                        WHERE NOT EXISTS (
                            SELECT 1 FROM morador_unidades mu WHERE mu.morador_id = m.id
                        )
                    ) THEN
                        RAISE EXCEPTION 'Morador sem ocupação não pode voltar ao modelo anterior';
                    END IF;
                END
                $mig$;

                CREATE TABLE pessoas (
                    id uuid NOT NULL DEFAULT gen_random_uuid(),
                    nome text NOT NULL,
                    email text NOT NULL,
                    telefone text NULL,
                    criado_em timestamp with time zone NOT NULL DEFAULT now(),
                    atualizado_em timestamp with time zone NULL,
                    CONSTRAINT "PK_pessoas" PRIMARY KEY (id)
                );

                INSERT INTO pessoas (id, nome, email, telefone, criado_em, atualizado_em)
                SELECT id, nome, email, telefone, criado_em, atualizado_em
                FROM usuarios;

                ALTER TABLE usuarios ADD COLUMN pessoa_id uuid;
                ALTER TABLE usuarios ADD COLUMN sindico_id uuid;
                ALTER TABLE usuarios ADD COLUMN cargo text;

                UPDATE usuarios SET pessoa_id = id;

                UPDATE usuarios u
                SET cargo = 'sindico',
                    sindico_id = NULL
                FROM sindicos s
                WHERE s.usuario_id = u.id;

                UPDATE usuarios u
                SET cargo = f.cargo,
                    sindico_id = emp.usuario_id
                FROM funcionarios f
                JOIN sindicos emp ON emp.id = f.sindico_id
                WHERE f.usuario_id = u.id
                  AND u.cargo IS NULL;

                ALTER TABLE moradores ADD COLUMN unidade_id uuid;
                ALTER TABLE moradores ADD COLUMN pessoa_id uuid;
                ALTER TABLE moradores ADD COLUMN papel text;
                ALTER TABLE moradores ADD COLUMN deletado_em timestamp with time zone;

                UPDATE moradores m
                SET unidade_id = mu.unidade_id,
                    papel = mu.papel,
                    deletado_em = mu.deletado_em,
                    pessoa_id = m.usuario_id,
                    criado_em = mu.criado_em,
                    atualizado_em = mu.atualizado_em
                FROM morador_unidades mu
                WHERE mu.id = m.id;

                DROP INDEX "IX_moradores_usuario_id";
                ALTER TABLE moradores DROP CONSTRAINT "FK_moradores_usuarios_usuario_id";

                INSERT INTO moradores (
                    id, usuario_id, criado_em, atualizado_em, unidade_id, pessoa_id, papel, deletado_em)
                SELECT
                    mu.id,
                    m.usuario_id,
                    mu.criado_em,
                    mu.atualizado_em,
                    mu.unidade_id,
                    m.usuario_id,
                    mu.papel,
                    mu.deletado_em
                FROM morador_unidades mu
                JOIN moradores m ON m.id = mu.morador_id
                WHERE NOT EXISTS (SELECT 1 FROM moradores existente WHERE existente.id = mu.id);

                UPDATE email_logs el
                SET morador_id = escolhida.id
                FROM moradores pessoa
                JOIN LATERAL (
                    SELECT o.id
                    FROM moradores o
                    WHERE o.usuario_id = pessoa.usuario_id
                      AND o.unidade_id IS NOT NULL
                      AND o.id <> pessoa.id
                    ORDER BY (o.deletado_em IS NULL) DESC, o.criado_em, o.id
                    LIMIT 1
                ) escolhida ON true
                WHERE el.morador_id = pessoa.id
                  AND pessoa.unidade_id IS NULL;

                UPDATE ocorrencias o
                SET morador_id = escolhida.id
                FROM moradores pessoa
                JOIN LATERAL (
                    SELECT ocupacao.id
                    FROM moradores ocupacao
                    WHERE ocupacao.usuario_id = pessoa.usuario_id
                      AND ocupacao.unidade_id IS NOT NULL
                      AND ocupacao.id <> pessoa.id
                    ORDER BY (ocupacao.deletado_em IS NULL) DESC, ocupacao.criado_em, ocupacao.id
                    LIMIT 1
                ) escolhida ON true
                WHERE o.morador_id = pessoa.id
                  AND pessoa.unidade_id IS NULL;

                DROP TABLE morador_unidades;

                DELETE FROM moradores WHERE unidade_id IS NULL;

                DO $mig$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM moradores
                        WHERE unidade_id IS NULL OR pessoa_id IS NULL OR papel IS NULL
                    ) THEN
                        RAISE EXCEPTION 'Ocupação de morador incompleta na reversão';
                    END IF;
                END
                $mig$;

                ALTER TABLE moradores DROP COLUMN usuario_id;
                ALTER TABLE moradores ALTER COLUMN unidade_id SET NOT NULL;
                ALTER TABLE moradores ALTER COLUMN pessoa_id SET NOT NULL;
                ALTER TABLE moradores ALTER COLUMN papel SET NOT NULL;

                ALTER TABLE condominios DROP CONSTRAINT "FK_condominios_sindicos_sindico_id";
                ALTER TABLE email_logs DROP CONSTRAINT "FK_email_logs_sindicos_sindico_id";
                ALTER TABLE email_templates DROP CONSTRAINT "FK_email_templates_sindicos_sindico_id";
                ALTER TABLE fornecedores DROP CONSTRAINT "FK_fornecedores_sindicos_sindico_id";

                UPDATE condominios c
                SET sindico_id = s.usuario_id
                FROM sindicos s
                WHERE s.id = c.sindico_id;

                UPDATE email_logs e
                SET sindico_id = s.usuario_id
                FROM sindicos s
                WHERE s.id = e.sindico_id;

                UPDATE email_templates t
                SET sindico_id = s.usuario_id
                FROM sindicos s
                WHERE s.id = t.sindico_id;

                UPDATE fornecedores f
                SET sindico_id = s.usuario_id
                FROM sindicos s
                WHERE s.id = f.sindico_id;

                ALTER TABLE condominios
                    ADD CONSTRAINT "FK_condominios_usuarios_sindico_id"
                    FOREIGN KEY (sindico_id) REFERENCES usuarios (id) ON DELETE RESTRICT;
                ALTER TABLE email_logs
                    ADD CONSTRAINT "FK_email_logs_usuarios_sindico_id"
                    FOREIGN KEY (sindico_id) REFERENCES usuarios (id) ON DELETE RESTRICT;
                ALTER TABLE email_templates
                    ADD CONSTRAINT "FK_email_templates_usuarios_sindico_id"
                    FOREIGN KEY (sindico_id) REFERENCES usuarios (id) ON DELETE RESTRICT;
                ALTER TABLE fornecedores
                    ADD CONSTRAINT "FK_fornecedores_usuarios_sindico_id"
                    FOREIGN KEY (sindico_id) REFERENCES usuarios (id) ON DELETE RESTRICT;

                CREATE TABLE usuario_condominios (
                    usuario_id uuid NOT NULL,
                    condominio_id uuid NOT NULL,
                    criado_em timestamp with time zone NOT NULL DEFAULT now(),
                    CONSTRAINT "PK_usuario_condominios" PRIMARY KEY (usuario_id, condominio_id),
                    CONSTRAINT "FK_usuario_condominios_condominios_condominio_id"
                        FOREIGN KEY (condominio_id) REFERENCES condominios (id) ON DELETE CASCADE,
                    CONSTRAINT "FK_usuario_condominios_usuarios_usuario_id"
                        FOREIGN KEY (usuario_id) REFERENCES usuarios (id) ON DELETE CASCADE
                );

                INSERT INTO usuario_condominios (usuario_id, condominio_id, criado_em)
                SELECT f.usuario_id, fc.condominio_id, fc.criado_em
                FROM funcionario_condominios fc
                JOIN funcionarios f ON f.id = fc.funcionario_id;

                CREATE INDEX "IX_usuario_condominios_condominio_id" ON usuario_condominios (condominio_id);

                DROP TABLE funcionario_condominios;
                DROP TABLE funcionarios;
                DROP TABLE sindicos;

                DO $mig$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM usuarios u
                        WHERE u.cargo IS NULL
                          AND (
                              EXISTS (SELECT 1 FROM solicitacoes s WHERE s.solicitado_por_id = u.id)
                              OR EXISTS (SELECT 1 FROM solicitacoes_compra sc WHERE sc.aprovado_por_id = u.id)
                              OR EXISTS (SELECT 1 FROM ocorrencias o WHERE o.registrado_por_id = u.id)
                              OR EXISTS (SELECT 1 FROM email_logs e WHERE e.enviado_por_id = u.id)
                              OR EXISTS (SELECT 1 FROM midias_ocorrencia m WHERE m.enviado_por_id = u.id)
                          )
                    ) THEN
                        RAISE EXCEPTION 'Usuário sem cargo é autor de registros e não pode ser removido na reversão';
                    END IF;

                    IF EXISTS (SELECT 1 FROM usuarios WHERE cargo IS NOT NULL AND pessoa_id IS NULL) THEN
                        RAISE EXCEPTION 'Usuário de acesso sem pessoa na reversão';
                    END IF;
                END
                $mig$;

                DELETE FROM usuarios WHERE cargo IS NULL;

                DROP INDEX "IX_usuarios_email";
                ALTER TABLE usuarios DROP COLUMN nome;
                ALTER TABLE usuarios DROP COLUMN email;
                ALTER TABLE usuarios DROP COLUMN telefone;

                ALTER TABLE usuarios ALTER COLUMN pessoa_id SET NOT NULL;
                ALTER TABLE usuarios ALTER COLUMN cargo SET NOT NULL;

                CREATE UNIQUE INDEX "IX_usuarios_pessoa_id" ON usuarios (pessoa_id);
                CREATE INDEX "IX_usuarios_sindico_id" ON usuarios (sindico_id);

                ALTER TABLE usuarios
                    ADD CONSTRAINT ck_usuarios_cargo
                    CHECK (cargo IN ('sindico', 'secretario', 'zelador', 'porteiro', 'outro'));
                ALTER TABLE usuarios
                    ADD CONSTRAINT ck_usuarios_sindico_carteira
                    CHECK ((cargo = 'sindico' AND sindico_id IS NULL) OR (cargo <> 'sindico' AND sindico_id IS NOT NULL));
                ALTER TABLE usuarios
                    ADD CONSTRAINT "FK_usuarios_pessoas_pessoa_id"
                    FOREIGN KEY (pessoa_id) REFERENCES pessoas (id) ON DELETE RESTRICT;
                ALTER TABLE usuarios
                    ADD CONSTRAINT "FK_usuarios_usuarios_sindico_id"
                    FOREIGN KEY (sindico_id) REFERENCES usuarios (id) ON DELETE RESTRICT;

                CREATE INDEX "IX_moradores_pessoa_id" ON moradores (pessoa_id);
                CREATE UNIQUE INDEX ix_moradores_proprietario_unico
                    ON moradores (unidade_id)
                    WHERE papel = 'proprietario' AND deletado_em IS NULL;
                CREATE UNIQUE INDEX ix_moradores_unidade_pessoa
                    ON moradores (unidade_id, pessoa_id)
                    WHERE deletado_em IS NULL;
                ALTER TABLE moradores
                    ADD CONSTRAINT ck_moradores_papel
                    CHECK (papel IN ('proprietario', 'inquilino', 'ocupante'));
                ALTER TABLE moradores
                    ADD CONSTRAINT "FK_moradores_pessoas_pessoa_id"
                    FOREIGN KEY (pessoa_id) REFERENCES pessoas (id) ON DELETE RESTRICT;
                ALTER TABLE moradores
                    ADD CONSTRAINT "FK_moradores_unidades_unidade_id"
                    FOREIGN KEY (unidade_id) REFERENCES unidades (id) ON DELETE RESTRICT;
                """);
        }
    }
}
