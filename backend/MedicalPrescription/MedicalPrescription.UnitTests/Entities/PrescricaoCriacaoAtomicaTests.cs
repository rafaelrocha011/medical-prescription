using MedicalPrescription.Domain.Entities;
using MedicalPrescription.Domain.Exceptions;
using Xunit;

namespace MedicalPrescription.UnitTests.Entities;

public sealed class PrescricaoCriacaoAtomicaTests
{
    [Fact]
    public void CriarEstruturada_ComItemDeOutraPrescricao_NaoDeveAlterarVinculos()
    {
        var vinculado = NovoItem();

        var origem = Prescricao.CriarEstruturada(
            Guid.NewGuid(), new[] { vinculado });

        var novo = NovoItem();

        Assert.Throws<RegraNegocioException>(() =>
            Prescricao.CriarEstruturada(
                Guid.NewGuid(), new[] { novo, vinculado }));

        Assert.Equal(Guid.Empty, novo.PrescricaoId);
        Assert.Equal(origem.Id, vinculado.PrescricaoId);
        Assert.Same(vinculado, Assert.Single(origem.Itens));
    }

    [Fact]
    public void CriarEstruturada_ComItemDuplicado_NaoDeveModificarVinculo()
    {
        var item = NovoItem();

        Assert.Throws<ArgumentException>(() =>
            Prescricao.CriarEstruturada(
                Guid.NewGuid(), new[] { item, item }));

        Assert.Equal(Guid.Empty, item.PrescricaoId);
    }

    [Fact]
    public void CriarEstruturada_ComItemNulo_NaoDeveModificarVinculo()
    {
        var item = NovoItem();

        Assert.Throws<ArgumentException>(() =>
            Prescricao.CriarEstruturada(
                Guid.NewGuid(),
                new PrescricaoItem[] { item, null! }));

        Assert.Equal(Guid.Empty, item.PrescricaoId);
    }

    [Fact]
    public void CriarEstruturada_ComItensValidos_DeveVincularTodos()
    {
        var primeiro = NovoItem();
        var segundo = NovoItem();

        var prescricao = Prescricao.CriarEstruturada(
            Guid.NewGuid(), new[] { primeiro, segundo });

        Assert.Equal(2, prescricao.Itens.Count);
        Assert.Equal(prescricao.Id, primeiro.PrescricaoId);
        Assert.Equal(prescricao.Id, segundo.PrescricaoId);
    }

    private static PrescricaoItem NovoItem()
    {
        return new PrescricaoItem(
            medicamentoId: null,
            nomeMedicamento: "Medicamento de teste",
            formaFarmaceutica: "Comprimido",
            dose: 250m,
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