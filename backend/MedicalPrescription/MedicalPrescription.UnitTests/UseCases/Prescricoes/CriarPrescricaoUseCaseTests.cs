
using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.DTOs.Prescricoes;
using MedicalPrescription.Application.Commands.Prescricoes.CriarPrescricao;
using MedicalPrescription.Domain.Entities;
using MedicalPrescription.Domain.Enums;
using Xunit;

namespace MedicalPrescription.UnitTests.UseCases.Prescricoes;

public class CriarPrescricaoUseCaseTests
{
    private readonly FakePacienteRepository _pacientes = new();
    private readonly FakeMedicamentoRepository _medicamentos = new();
    private readonly FakePrescricaoWriteRepository _prescricoes = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private readonly CriarPrescricaoUseCase _useCase;
    private readonly Guid _pacienteId = Guid.NewGuid();

    public CriarPrescricaoUseCaseTests()
    {
        _useCase = new CriarPrescricaoUseCase(
            _pacientes,
            _medicamentos,
            _prescricoes,
            _unitOfWork);
    }

    [Fact]
    public async Task CriarNaoEstruturada_ComDadosValidos_DevePersistir()
    {
        var request = new CriarPrescricaoRequest(
            _pacienteId,
            TipoPrescricao.NaoEstruturada,
            "Receita em texto livre",
            null);

        var id = await _useCase.ExecutarAsync(request);

        var prescricao = Assert.Single(_prescricoes.Adicionadas);

        Assert.Equal(id, prescricao.Id);
        Assert.Equal(TipoPrescricao.NaoEstruturada, prescricao.Tipo);
        Assert.Equal("Receita em texto livre", prescricao.TextoLivre);
        Assert.Equal(1, _unitOfWork.Chamadas);
    }

    [Fact]
    public async Task CriarEstruturada_ComMedicamentoCadastrado_DevePersistir()
    {
        var medicamento = new Medicamento("Dipirona");
        _medicamentos.Catalogo.Add(medicamento.Id, medicamento);

        var request = NovaPrescricaoEstruturada(
            NovoItem(medicamento.Id));

        var id = await _useCase.ExecutarAsync(request);

        var prescricao = Assert.Single(_prescricoes.Adicionadas);
        var item = Assert.Single(prescricao.Itens);

        Assert.Equal(id, prescricao.Id);
        Assert.Equal(medicamento.Id, item.MedicamentoId);
        Assert.Equal("Dipirona", item.NomeMedicamento);
        Assert.Equal(1, _unitOfWork.Chamadas);
    }

    [Fact]
    public async Task CriarEstruturada_ComMedicamentoNaoCadastrado_DevePermitir()
    {
        var request = NovaPrescricaoEstruturada(NovoItem());

        await _useCase.ExecutarAsync(request);

        var prescricao = Assert.Single(_prescricoes.Adicionadas);
        var item = Assert.Single(prescricao.Itens);

        Assert.Null(item.MedicamentoId);
        Assert.Equal("Medicamento manipulado", item.NomeMedicamento);
        Assert.Equal(0, _medicamentos.Consultas);
    }

    [Fact]
    public async Task Criar_ComPacienteInexistente_DeveRejeitar()
    {
        _pacientes.Existe = false;

        var request = new CriarPrescricaoRequest(
            _pacienteId,
            TipoPrescricao.NaoEstruturada,
            "Receita em texto livre",
            null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _useCase.ExecutarAsync(request));

        Assert.Empty(_prescricoes.Adicionadas);
        Assert.Equal(0, _unitOfWork.Chamadas);
    }

    [Fact]
    public async Task Criar_ComMedicamentoInexistente_DeveRejeitar()
    {
        var request = NovaPrescricaoEstruturada(
            NovoItem(Guid.NewGuid()));

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _useCase.ExecutarAsync(request));

        Assert.Empty(_prescricoes.Adicionadas);
        Assert.Equal(0, _unitOfWork.Chamadas);
    }

    [Fact]
    public async Task CriarNaoEstruturada_SemTexto_DeveRejeitar()
    {
        var request = new CriarPrescricaoRequest(
            _pacienteId,
            TipoPrescricao.NaoEstruturada,
            "",
            null);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _useCase.ExecutarAsync(request));

        Assert.Empty(_prescricoes.Adicionadas);
        Assert.Equal(0, _unitOfWork.Chamadas);
    }

    [Fact]
    public async Task CriarEstruturada_ComDoseInvalida_DeveRejeitar()
    {
        var item = NovoItem() with { Dose = 0 };
        var request = NovaPrescricaoEstruturada(item);

        await Assert.ThrowsAsync<ArgumentException>(
            () => _useCase.ExecutarAsync(request));

        Assert.Empty(_prescricoes.Adicionadas);
        Assert.Equal(0, _unitOfWork.Chamadas);
    }

    private CriarPrescricaoRequest NovaPrescricaoEstruturada(
        CriarPrescricaoItemRequest item)
    {
        return new CriarPrescricaoRequest(
            _pacienteId,
            TipoPrescricao.Estruturada,
            null,
            new[] { item });
    }

    private static CriarPrescricaoItemRequest NovoItem(
        Guid? medicamentoId = null)
    {
        return new CriarPrescricaoItemRequest(
            MedicamentoId: medicamentoId,
            NomeMedicamento: "Medicamento manipulado",
            FormaFarmaceutica: "Comprimido",
            Dose: 500,
            UnidadeMedida: "mg",
            Frequencia: "A cada 8 horas",
            InicioTratamento: new DateOnly(2026, 11, 17),
            DuracaoTratamentoDias: 7,
            UsoContinuo: false,
            ViaAdministracao: "Oral",
            PrazoUtilizacao: "7 dias",
            Observacao: null);
    }

    private sealed class FakePacienteRepository : IPacienteRepository
    {
        public Task<IReadOnlyList<MedicalPrescription.Application.DTOs.Catalogos.PacienteResumoDto>> ListarAsync(
            string? busca,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<MedicalPrescription.Application.DTOs.Catalogos.PacienteResumoDto>>(
                Array.Empty<MedicalPrescription.Application.DTOs.Catalogos.PacienteResumoDto>());
        }

        public bool Existe { get; set; } = true;

        public Task<bool> ExisteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Existe);
    }

    private sealed class FakeMedicamentoRepository : IMedicamentoRepository
    {
        public Task<IReadOnlyList<MedicalPrescription.Application.DTOs.Catalogos.MedicamentoResumoDto>> ListarAsync(
            string? busca,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<MedicalPrescription.Application.DTOs.Catalogos.MedicamentoResumoDto>>(
                Array.Empty<MedicalPrescription.Application.DTOs.Catalogos.MedicamentoResumoDto>());
        }

        public Dictionary<Guid, Medicamento> Catalogo { get; } = new();
        public int Consultas { get; private set; }

        public Task<Medicamento?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            Consultas++;
            return Task.FromResult(Catalogo.GetValueOrDefault(id));
        }
    }

    private sealed class FakePrescricaoWriteRepository : IPrescricaoWriteRepository
    {
        public List<Prescricao> Adicionadas { get; } = new();

        public Task<Prescricao?> ObterParaEscritaAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Adicionadas.FirstOrDefault(p => p.Id == id));
        }

        public Task AdicionarAsync(
            Prescricao prescricao,
            CancellationToken cancellationToken = default)
        {
            Adicionadas.Add(prescricao);
            return Task.CompletedTask;
        }

        public void Remover(Prescricao prescricao)
            => Adicionadas.Remove(prescricao);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int Chamadas { get; private set; }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            Chamadas++;
            return Task.FromResult(1);
        }
    }
}
