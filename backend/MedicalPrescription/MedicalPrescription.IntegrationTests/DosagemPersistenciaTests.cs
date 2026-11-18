using MedicalPrescription.Domain.Entities;
using MedicalPrescription.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MedicalPrescription.IntegrationTests;

public sealed class DosagemPersistenciaTests
    : IClassFixture<MySqlFixture>
{
    private readonly MySqlFixture _fixture;

    public DosagemPersistenciaTests(MySqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task DosagensComTresCasas_DevemPersistirSemArredondar()
    {
        var paciente = new Paciente("Paciente Precisao");

        var prescricao = Prescricao.CriarEstruturada(
            paciente.Id,
            new[]
            {
                NovoItem("Dose minima", 0.001m),
                NovoItem("Dose fracionaria", 12.345m)
            });

        await using (var context = _fixture.NovoContexto())
        {
            context.Pacientes.Add(paciente);
            context.Prescricoes.Add(prescricao);

            await context.SaveChangesAsync();
        }

        await using (var context = _fixture.NovoContexto())
        {
            var itens = await context.PrescricaoItens
                .AsNoTracking()
                .Where(x => x.PrescricaoId == prescricao.Id)
                .ToListAsync();

            Assert.Equal(2, itens.Count);

            Assert.Equal(
                0.001m,
                itens.Single(x =>
                    x.NomeMedicamento == "Dose minima")
                    .Dosagem.Quantidade);

            Assert.Equal(
                12.345m,
                itens.Single(x =>
                    x.NomeMedicamento == "Dose fracionaria")
                    .Dosagem.Quantidade);
        }
    }

    private static PrescricaoItem NovoItem(
        string nome,
        decimal dose)
    {
        return new PrescricaoItem(
            medicamentoId: null,
            nomeMedicamento: nome,
            formaFarmaceutica: "Comprimido",
            dose: dose,
            unidadeMedida: "mg",
            frequencia: "A cada 8 horas",
            inicioTratamento: new DateOnly(2026, 11, 17),
            duracaoTratamentoDias: 7,
            usoContinuo: false,
            viaAdministracao: "Oral",
            prazoUtilizacao: "7 dias");
    }
}