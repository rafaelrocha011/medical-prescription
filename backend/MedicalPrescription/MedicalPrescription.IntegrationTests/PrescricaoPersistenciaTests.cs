using MedicalPrescription.Domain.Entities;
using MedicalPrescription.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MedicalPrescription.IntegrationTests;

public sealed class PrescricaoPersistenciaTests
    : IClassFixture<MySqlFixture>
{
    private readonly MySqlFixture _fixture;

    public PrescricaoPersistenciaTests(MySqlFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SubstituirItens_DeveExcluirAntigosEPersistirNovos()
    {
        // Arrange: criar uma prescricao com um medicamento.
        var paciente = new Paciente("Paciente Integracao");

        var prescricao = Prescricao.CriarEstruturada(
            paciente.Id,
            new[] { NovoItem("Original", 100m) });

        var idOriginal = prescricao.Itens.Single().Id;

        await using (var context = _fixture.NovoContexto())
        {
            context.Pacientes.Add(paciente);

            var repository = new PrescricaoWriteRepository(context);

            await repository.AdicionarAsync(prescricao);
            await context.SaveChangesAsync();
        }

        // Act: carregar agregado rastreado e substituir os itens.
        await using (var context = _fixture.NovoContexto())
        {
            var repository = new PrescricaoWriteRepository(context);

            var agregado = await repository.ObterParaEscritaAsync(
                prescricao.Id);

            Assert.NotNull(agregado);

            agregado.SubstituirItens(new[]
            {
                NovoItem("Novo medicamento A", 250m),
                NovoItem("Novo medicamento B", 500m)
            });

            await context.SaveChangesAsync();
        }

        // Assert: consultar com novo DbContext para verificar o banco.
        await using (var context = _fixture.NovoContexto())
        {
            var itens = await context.PrescricaoItens
                .AsNoTracking()
                .Where(i => i.PrescricaoId == prescricao.Id)
                .ToListAsync();

            Assert.Equal(2, itens.Count);

            Assert.DoesNotContain(
                itens,
                item => item.Id == idOriginal);

            Assert.Contains(
                itens,
                item =>
                    item.NomeMedicamento == "Novo medicamento A" &&
                    item.Dosagem.Quantidade == 250m);

            Assert.Contains(
                itens,
                item =>
                    item.NomeMedicamento == "Novo medicamento B" &&
                    item.Dosagem.Quantidade == 500m);

            Assert.All(
                itens,
                item => Assert.Equal(prescricao.Id, item.PrescricaoId));
        }
    }

    [Fact]
    public async Task ExcluirPrescricao_DeveExcluirItensEPreservarPaciente()
    {
        // Arrange
        var paciente = new Paciente("Paciente Exclusao");

        var prescricao = Prescricao.CriarEstruturada(
            paciente.Id,
            new[] { NovoItem("Medicamento teste", 100m) });

        await using (var context = _fixture.NovoContexto())
        {
            context.Pacientes.Add(paciente);
            context.Prescricoes.Add(prescricao);

            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = _fixture.NovoContexto())
        {
            var repository = new PrescricaoWriteRepository(context);

            var agregado = await repository.ObterParaEscritaAsync(
                prescricao.Id);

            Assert.NotNull(agregado);

            repository.Remover(agregado);

            await context.SaveChangesAsync();
        }

        // Assert
        await using (var context = _fixture.NovoContexto())
        {
            Assert.False(await context.Prescricoes
                .AnyAsync(p => p.Id == prescricao.Id));

            Assert.False(await context.PrescricaoItens
                .AnyAsync(i => i.PrescricaoId == prescricao.Id));

            Assert.True(await context.Pacientes
                .AnyAsync(p => p.Id == paciente.Id));
        }
    }

    [Fact]
    public async Task ConsultarDetalhes_NaoDeveRastrearEntidades()
    {
        // Arrange
        var paciente = new Paciente("Paciente Consulta");

        var prescricao = Prescricao.CriarEstruturada(
            paciente.Id,
            new[] { NovoItem("Medicamento consulta", 300m) });

        await using (var context = _fixture.NovoContexto())
        {
            context.Pacientes.Add(paciente);
            context.Prescricoes.Add(prescricao);

            await context.SaveChangesAsync();
        }

        // Act
        await using (var context = _fixture.NovoContexto())
        {
            var repository = new PrescricaoReadRepository(context);

            var resultado = await repository.ObterPorIdAsync(
                prescricao.Id);

            // Assert
            Assert.NotNull(resultado);

            Assert.Equal(prescricao.Id, resultado.Id);
            Assert.Equal(paciente.Id, resultado.PacienteId);

            var item = Assert.Single(resultado.Itens);

            Assert.Equal("Medicamento consulta", item.NomeMedicamento);
            Assert.Equal(300m, item.Dose);
            Assert.Equal("mg", item.UnidadeMedida);

            Assert.Empty(context.ChangeTracker.Entries());
        }
    }

    [Fact]

    public async Task ListarEConsultar_DeveRetornarNomeDoPaciente()

    {

        var paciente = new Paciente("Paciente Nome Completo Teste");

        var prescricao = Prescricao.CriarNaoEstruturada(

            paciente.Id, "Texto de teste");



        await using (var context = _fixture.NovoContexto())

        {

            context.Pacientes.Add(paciente);

            context.Prescricoes.Add(prescricao);

            await context.SaveChangesAsync();

        }



        await using (var context = _fixture.NovoContexto())

        {

            var repo = new PrescricaoReadRepository(context);

            var pagina = await repo.ListarAsync(1, 100);

            var resumo = Assert.Single(

                pagina.Itens.Where(x => x.Id == prescricao.Id));

            Assert.Equal(paciente.NomeCompleto, resumo.NomePaciente);



            var detalhe = await repo.ObterPorIdAsync(prescricao.Id);

            Assert.NotNull(detalhe);

            Assert.Equal(paciente.NomeCompleto, detalhe.NomePaciente);

            Assert.Empty(context.ChangeTracker.Entries());

        }

    }


    private static PrescricaoItem NovoItem(string nome, decimal dose)
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
            prazoUtilizacao: "7 dias",
            observacao: null);
    }
}