CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `medicamentos` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `nome` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_medicamentos` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `pacientes` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `nome_completo` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_pacientes` PRIMARY KEY (`id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `prescricoes` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `paciente_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `tipo` int NOT NULL,
    `texto_livre` longtext CHARACTER SET utf8mb4 NULL,
    `criada_em_utc` datetime(6) NOT NULL,
    CONSTRAINT `PK_prescricoes` PRIMARY KEY (`id`),
    CONSTRAINT `FK_prescricoes_pacientes_paciente_id` FOREIGN KEY (`paciente_id`) REFERENCES `pacientes` (`id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `prescricao_itens` (
    `id` char(36) COLLATE ascii_general_ci NOT NULL,
    `prescricao_id` char(36) COLLATE ascii_general_ci NOT NULL,
    `medicamento_id` char(36) COLLATE ascii_general_ci NULL,
    `nome_medicamento` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `forma_farmaceutica` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `dose` decimal(12,3) NOT NULL,
    `unidade_medida` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `frequencia` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `inicio_tratamento` date NOT NULL,
    `duracao_tratamento_dias` int NULL,
    `uso_continuo` tinyint(1) NOT NULL,
    `via_administracao` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `prazo_utilizacao` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `observacao` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_prescricao_itens` PRIMARY KEY (`id`),
    CONSTRAINT `FK_prescricao_itens_medicamentos_medicamento_id` FOREIGN KEY (`medicamento_id`) REFERENCES `medicamentos` (`id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_prescricao_itens_prescricoes_prescricao_id` FOREIGN KEY (`prescricao_id`) REFERENCES `prescricoes` (`id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_prescricao_itens_medicamento_id` ON `prescricao_itens` (`medicamento_id`);

CREATE INDEX `IX_prescricao_itens_prescricao_id` ON `prescricao_itens` (`prescricao_id`);

CREATE INDEX `IX_prescricoes_criada_em_utc` ON `prescricoes` (`criada_em_utc`);

CREATE INDEX `IX_prescricoes_paciente_id` ON `prescricoes` (`paciente_id`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20261117042749_CriacaoInicial', '9.0.23');

COMMIT;

