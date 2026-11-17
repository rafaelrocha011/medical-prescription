using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalPrescription.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CriacaoInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "medicamentos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    nome = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medicamentos", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pacientes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    nome_completo = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pacientes", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "prescricoes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    paciente_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    tipo = table.Column<int>(type: "int", nullable: false),
                    texto_livre = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    criada_em_utc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prescricoes", x => x.id);
                    table.ForeignKey(
                        name: "FK_prescricoes_pacientes_paciente_id",
                        column: x => x.paciente_id,
                        principalTable: "pacientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "prescricao_itens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    prescricao_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    medicamento_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    nome_medicamento = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    forma_farmaceutica = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dose = table.Column<decimal>(type: "decimal(12,3)", precision: 12, scale: 3, nullable: false),
                    unidade_medida = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    frequencia = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    inicio_tratamento = table.Column<DateOnly>(type: "date", nullable: false),
                    duracao_tratamento_dias = table.Column<int>(type: "int", nullable: true),
                    uso_continuo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    via_administracao = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    prazo_utilizacao = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    observacao = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prescricao_itens", x => x.id);
                    table.ForeignKey(
                        name: "FK_prescricao_itens_medicamentos_medicamento_id",
                        column: x => x.medicamento_id,
                        principalTable: "medicamentos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prescricao_itens_prescricoes_prescricao_id",
                        column: x => x.prescricao_id,
                        principalTable: "prescricoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_prescricao_itens_medicamento_id",
                table: "prescricao_itens",
                column: "medicamento_id");

            migrationBuilder.CreateIndex(
                name: "IX_prescricao_itens_prescricao_id",
                table: "prescricao_itens",
                column: "prescricao_id");

            migrationBuilder.CreateIndex(
                name: "IX_prescricoes_criada_em_utc",
                table: "prescricoes",
                column: "criada_em_utc");

            migrationBuilder.CreateIndex(
                name: "IX_prescricoes_paciente_id",
                table: "prescricoes",
                column: "paciente_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prescricao_itens");

            migrationBuilder.DropTable(
                name: "medicamentos");

            migrationBuilder.DropTable(
                name: "prescricoes");

            migrationBuilder.DropTable(
                name: "pacientes");
        }
    }
}
