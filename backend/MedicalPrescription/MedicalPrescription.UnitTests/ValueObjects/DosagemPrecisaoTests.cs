using System.Globalization;
using MedicalPrescription.Domain.ValueObjects;
using Xunit;

namespace MedicalPrescription.UnitTests.ValueObjects;

public sealed class DosagemPrecisaoTests
{
    [Theory]
    [InlineData("0.0001")]
    [InlineData("1.2345")]
    public void QuantidadeComMaisDeTresCasas_DeveRejeitar(
        string valor)
    {
        var quantidade = decimal.Parse(
            valor, CultureInfo.InvariantCulture);

        Assert.Throws<ArgumentException>(() =>
            new Dosagem(quantidade, "mg"));
    }

    [Fact]
    public void QuantidadeAcimaDoLimite_DeveRejeitar()
    {
        Assert.Throws<ArgumentException>(() =>
            new Dosagem(1_000_000_000m, "mg"));
    }

    [Fact]
    public void QuantidadeNoLimite_DevePermitir()
    {
        var dosagem = new Dosagem(
            Dosagem.QuantidadeMaxima, "mg");

        Assert.Equal(
            999_999_999.999m,
            dosagem.Quantidade);
    }

    [Fact]
    public void QuantidadeComZerosDecimaisExtras_DevePermitir()
    {
        var dosagem = new Dosagem(1.23000m, "mg");

        Assert.Equal(1.23m, dosagem.Quantidade);
    }

    [Fact]
    public void UnidadeMuitoLonga_DeveRejeitar()
    {
        var unidade = new string('x', 21);

        Assert.Throws<ArgumentException>(() =>
            new Dosagem(500m, unidade));
    }

    [Fact]
    public void UnidadeComEspacos_DeveSerNormalizada()
    {
        var dosagem = new Dosagem(500m, " mg ");

        Assert.Equal("mg", dosagem.UnidadeMedida);
    }

    [Fact]
    public void UnidadeNoTamanhoMaximo_DevePermitir()
    {
        var unidade = new string('x', 20);

        var dosagem = new Dosagem(500m, unidade);

        Assert.Equal(unidade, dosagem.UnidadeMedida);
    }
}