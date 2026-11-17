using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.DTOs.Prescricoes;
using MedicalPrescription.Application.UseCases.Prescricoes.AtualizarPrescricao;
using MedicalPrescription.Domain.Entities;
using Xunit;

namespace MedicalPrescription.UnitTests.UseCases.Prescricoes;

public class AtualizarPrescricaoUseCaseTests
{
    private readonly FakePrescricaoRepository _prescricoes = new();
    private readonly FakeMedicamentoRepository _medicamentos = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private readonly AtualizarPrescricaoUseCase _useCase;

    public AtualizarPrescricaoUseCaseTests()
    {
        _useCase = new AtualizarPrescricaoUseCase(
            _prescricoes,
            _medicamentos,
            _unitOfWork);
    }

    [Fact]
    public async Task AtualizarNaoEstruturada_DeveAlterarTexto()
    {
        var prescricao = Prescricao.CriarNaoEstruturada(
            Guid.NewGuid(),
            "Texto original");

        _prescricoes.Adicionar(prescricao);

        var request = new AtualizarPrescricaoRequest(
            "Texto atualizado",
            null);

        await _useCase.ExecutarAsync(prescricao.Id, request);

        Assert.Equal("Texto atualizado", prescricao.TextoLivre);
        Assert.Equal(1, _unitOfWork.Chamadas);
    }

    [Fact]
    public async Task AtualizarEstruturada_DeveSubstituirMedicamentos()
    {
        var prescricao = CriarPrescricaoEstruturada();
        var itemOriginal = Assert.Single(prescricao.Itens);

        _prescricoes.Adicionar(prescricao);

        var medicamento = new Medicamento("Dipirona");
        _medicamentos.Catalogo.Add(medicamento.Id, medicamento);

        var request = new AtualizarPrescricaoRequest(
            null,
            new[] { NovoItemRequest(medicamento.Id) });

        await _useCase.ExecutarAsync(prescricao.Id, request);

        var itemAtualizado = Assert.Single(prescricao.Itens);

        Assert.NotEqual(itemOriginal.Id, itemAtualizado.Id);
        Assert.Equal("Dipirona", itemAtualizado.NomeMedicamento);
        Assert.Equal(medicamento.Id, itemAtualizado.MedicamentoId);
        Assert.Equal(prescricao.Id, itemAtualizado.PrescricaoId);
        Assert.Equal(1, _unitOfWork.Chamadas);
    }

    [Fact]
    public async Task AtualizarEstruturada_ComMedicamentoManual_DevePermitir()
    {
        var prescricao = CriarPrescricaoEstruturada();
        _prescricoes.Adicionar(prescricao);

        var request = new AtualizarPrescricaoRequest(
            null,
            new[] { NovoItemRequest() });

        await _useCase.ExecutarAsync(prescricao.Id, request);

        var item = Assert.Single(prescricao.Itens);

        Assert.Null(item.MedicamentoId);
        Assert.Equal("Medicamento manipulado", item.NomeMedicamento);
        Assert.Equal(1, _unitOfWork.Chamadas);
    }

    [Fact]
    public async Task AtualizarPrescricaoInexistente_DeveLancarExcecao()
    {
        var request = new AtualizarPrescricaoRequest(
            "Novo texto",
            null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _useCase.ExecutarAsync(Guid.NewGuid(), request));

        Assert.Equal(0, _unitOfWork.Chamadas);
    }

    [Fact]
    public async Task AtualizarNaoEstruturada_ComItens_DeveRejeitar()
    {
        var prescricao = Prescricao.CriarNaoEstruturada(
            Guid.NewGuid(),
            "Texto original");

        _prescricoes.Adicionar(prescricao);

        var request = new AtualizarPrescricaoRequest(
            "Novo texto",
            new[] { NovoItemRequest() });

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _useCase.ExecutarAsync(prescricao.Id, request));

        Assert.Equal("Texto original", prescricao.TextoLivre);
        Assert.Equal(0, _unitOfWork.Chamadas);
    }

    [Fact]
    public async Task AtualizarEstruturada_SemItens_DeveRejeitar()
    {
        var prescricao = CriarPrescricaoEstruturada();
        var itemOriginal = Assert.Single(prescricao.Itens);

        _prescricoes.Adicionar(prescricao);

        var request = new AtualizarPrescricaoRequest(
            null,
            Array.Empty<CriarPrescricaoItemRequest>());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _useCase.ExecutarAsync(prescricao.Id, request));

        Assert.Equal(itemOriginal.Id, Assert.Single(prescricao.Itens).Id);
        Assert.Equal(0, _unitOfWork.Chamadas);
    }

    [Fact]
    public async Task AtualizarEstruturada_ComMedicamentoInexistente_DeveRejeitar()
    {
        var prescricao = CriarPrescricaoEstruturada();
        var itemOriginal = Assert.Single(prescricao.Itens);

        _prescricoes.Adicionar(prescricao);

        var request = new AtualizarPrescricaoRequest(
            null,
            new[] { NovoItemRequest(Guid.NewGuid()) });

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _useCase.ExecutarAsync(prescricao.Id, request));

        Assert.Equal(itemOriginal.Id, Assert.Single(prescricao.Itens).Id);
        Assert.Equal(0, _unitOfWork.Chamadas);
    }

    [Fact]
    public async Task AtualizarEstruturada_ComTextoLivre_DeveRejeitar()
    {
        var prescricao = CriarPrescricaoEstruturada();
        _prescricoes.Adicionar(prescricao);

        var request = new AtualizarPrescricaoRequest(
            "Texto nao permitido",
            new[] { NovoItemRequest() });

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _useCase.ExecutarAsync(prescricao.Id, request));

        Assert.Equal(0, _unitOfWork.Chamadas);
    }

    private static Prescricao CriarPrescricaoEstruturada()
    {
        var item = new PrescricaoItem(
            medicamentoId: null,
            nomeMedicamento: "Medicamento original",
            formaFarmaceutica: "Comprimido",
            dose: 250,
            unidadeMedida: "mg",
            frequencia: "A cada 12 horas",
            inicioTratamento: new DateOnly(2026, 11, 17),
            duracaoTratamentoDias: 5,
            usoContinuo: false,
            viaAdministracao: "Oral",
            prazoUtilizacao: "5 dias"
        );

        return Prescricao.CriarEstruturada(
            Guid.NewGuid(),
            new[] { item });
    }

    private static CriarPrescricaoItemRequest NovoItemRequest(
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

    private sealed class FakePrescricaoRepository : IPrescricaoRepository
    {
        private readonly Dictionary<Guid, Prescricao> _dados = new();

        public void Adicionar(Prescricao prescricao)
        {
            _dados[prescricao.Id] = prescricao;
        }

        public Task<Prescricao?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_dados.GetValueOrDefault(id));
        }

        public Task AdicionarAsync(
            Prescricao prescricao,
            CancellationToken cancellationToken = default)
        {
            Adicionar(prescricao);
            return Task.CompletedTask;
        }

        public void Remover(Prescricao prescricao)
        {
            _dados.Remove(prescricao.Id);
        }
    }

    private sealed class FakeMedicamentoRepository : IMedicamentoRepository
    {
        public Dictionary<Guid, Medicamento> Catalogo { get; } = new();

        public Task<Medicamento?> ObterPorIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Catalogo.GetValueOrDefault(id));
        }
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
