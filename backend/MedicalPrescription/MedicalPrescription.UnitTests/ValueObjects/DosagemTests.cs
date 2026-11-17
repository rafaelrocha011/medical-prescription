
using MedicalPrescription.Domain.ValueObjects;

namespace MedicalPrescription.UnitTests.ValueObjects;

public class DosagemTests
{
    [Fact]
    public void Criar_ComValoresValidos_DeveCriarDosagem()
    {
        var dosagem = new Dosagem(500, "mg");

        Assert.Equal(500, dosagem.Quantidade);
        Assert.Equal("mg", dosagem.UnidadeMedida);
    }

    [Fact]
    public void Criar_ComQuantidadeZero_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentException>(() =>
            new Dosagem(0, "mg"));
    }

    [Fact]
    public void Criar_SemUnidadeMedida_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentException>(() =>
            new Dosagem(500, ""));
    }

    [Fact]
    public void DosagensComMesmosValores_DevemSerIguais()
    {
        var primeira = new Dosagem(500, "mg");
        var segunda = new Dosagem(500, "mg");

        Assert.Equal(primeira, segunda);
    }
}
