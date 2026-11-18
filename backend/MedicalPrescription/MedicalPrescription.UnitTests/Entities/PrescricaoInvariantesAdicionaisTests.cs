using MedicalPrescription.Domain.Exceptions;
using MedicalPrescription.Domain.Entities;
using MedicalPrescription.Domain.ValueObjects;
using Xunit;

namespace MedicalPrescription.UnitTests.Entities;

public sealed class PrescricaoInvariantesAdicionaisTests
{
    [Fact]
    public void SubstituirItens_ComItemDeOutraPrescricao_NaoDeveAlterarVinculos()
    {
        var original = NovoItem();

        var destino = Prescricao.CriarEstruturada(
            Guid.NewGuid(), new[] { original });

        var itemDeOutraPrescricao = NovoItem();

        var outra = Prescricao.CriarEstruturada(
            Guid.NewGuid(), new[] { itemDeOutraPrescricao });

        var novoItemValido = NovoItem();

        Assert.Throws<RegraNegocioException>(() =>
            destino.SubstituirItens(
                new[] { novoItemValido, itemDeOutraPrescricao }));

        Assert.Equal(Guid.Empty, novoItemValido.PrescricaoId);
        Assert.Equal(outra.Id, itemDeOutraPrescricao.PrescricaoId);
        Assert.Same(original, Assert.Single(destino.Itens));
    }

    [Fact]
    public void SubstituirItens_ComItemDuplicado_DevePreservarOriginal()
    {
        var original = NovoItem();

        var prescricao = Prescricao.CriarEstruturada(
            Guid.NewGuid(), new[] { original });

        var duplicado = NovoItem();

        Assert.Throws<ArgumentException>(() =>
            prescricao.SubstituirItens(
                new[] { duplicado, duplicado }));

        Assert.Same(original, Assert.Single(prescricao.Itens));
        Assert.Equal(Guid.Empty, duplicado.PrescricaoId);
    }

    [Fact]
    public void SubstituirItens_ComItensValidos_DeveAtualizarVinculos()
    {
        var prescricao = Prescricao.CriarEstruturada(
            Guid.NewGuid(), new[] { NovoItem() });

        var primeiro = NovoItem();
        var segundo = NovoItem();

        prescricao.SubstituirItens(new[] { primeiro, segundo });

        Assert.Equal(2, prescricao.Itens.Count);
        Assert.Equal(prescricao.Id, primeiro.PrescricaoId);
        Assert.Equal(prescricao.Id, segundo.PrescricaoId);
    }

    [Fact]
    public void AdicionarItem_DeOutraPrescricao_DeveRejeitar()
    {
        var item = NovoItem();

        Prescricao.CriarEstruturada(
            Guid.NewGuid(), new[] { item });

        var destino = Prescricao.CriarEstruturada(
            Guid.NewGuid(), new[] { NovoItem() });

        Assert.Throws<RegraNegocioException>(() =>
            destino.AdicionarItem(item));

        Assert.Single(destino.Itens);
    }

    [Fact]
    public void TratamentoNaoContinuo_SemDuracao_DeveRejeitar()
    {
        Assert.Throws<ArgumentException>(() =>
            NovoItem(usoContinuo: false, duracao: null));
    }

    [Fact]
    public void TratamentoContinuo_SemDuracao_DevePermitir()
    {
        var item = NovoItem(usoContinuo: true, duracao: null);

        Assert.True(item.UsoContinuo);
        Assert.Null(item.DuracaoTratamentoDias);
    }

    [Fact]
    public void Tratamento_ComDuracaoNegativa_DeveRejeitar()
    {
        Assert.Throws<ArgumentException>(() =>
            NovoItem(usoContinuo: false, duracao: -1));
    }

    [Fact]
    public void Dosagem_DeveRejeitarQuantidadeNegativaENormalizarUnidade()
    {
        Assert.Throws<ArgumentException>(() =>
            new Dosagem(-1m, "mg"));

        var dosagem = new Dosagem(250m, " mg ");

        Assert.Equal(250m, dosagem.Quantidade);
        Assert.Equal("mg", dosagem.UnidadeMedida);
    }

    private static PrescricaoItem NovoItem(
        bool usoContinuo = false,
        int? duracao = 7)
    {
        return new PrescricaoItem(
            medicamentoId: null,
            nomeMedicamento: "Medicamento de teste",
            formaFarmaceutica: "Comprimido",
            dose: 500m,
            unidadeMedida: "mg",
            frequencia: "A cada 8 horas",
            inicioTratamento: new DateOnly(2026, 11, 17),
            duracaoTratamentoDias: duracao,
            usoContinuo: usoContinuo,
            viaAdministracao: "Oral",
            prazoUtilizacao: "7 dias",
            observacao: null);
    }
}