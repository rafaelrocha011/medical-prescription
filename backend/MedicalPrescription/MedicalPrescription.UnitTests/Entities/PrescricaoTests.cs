using MedicalPrescription.Domain.Exceptions;

using MedicalPrescription.Domain.Entities;
using MedicalPrescription.Domain.Enums;
using Xunit;

namespace MedicalPrescription.UnitTests.Entities;

public class PrescricaoTests
{
    private static readonly Guid PacienteId = Guid.NewGuid();

    [Fact]
    public void CriarNaoEstruturada_ComTextoValido_DeveCriarPrescricao()
    {
        // Arrange
        const string texto = "Prescrever medicamento conforme orientação.";

        // Act
        var prescricao = Prescricao.CriarNaoEstruturada(
            PacienteId,
            texto);

        // Assert
        Assert.NotEqual(Guid.Empty, prescricao.Id);
        Assert.Equal(PacienteId, prescricao.PacienteId);
        Assert.Equal(TipoPrescricao.NaoEstruturada, prescricao.Tipo);
        Assert.Equal(texto, prescricao.TextoLivre);
        Assert.Empty(prescricao.Itens);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void CriarNaoEstruturada_SemTexto_DeveLancarExcecao(
        string? texto)
    {
        Assert.Throws<ArgumentException>(() =>
            Prescricao.CriarNaoEstruturada(PacienteId, texto!));
    }

    [Fact]
    public void CriarEstruturada_ComMedicamento_DeveCriarPrescricao()
    {
        // Arrange
        var item = CriarItemValido();

        // Act
        var prescricao = Prescricao.CriarEstruturada(
            PacienteId,
            new[] { item });

        // Assert
        Assert.Equal(TipoPrescricao.Estruturada, prescricao.Tipo);
        Assert.Null(prescricao.TextoLivre);
        Assert.Single(prescricao.Itens);
        Assert.Equal(prescricao.Id, item.PrescricaoId);
    }

    [Fact]
    public void CriarEstruturada_SemMedicamentos_DeveLancarExcecao()
    {
        Assert.Throws<ArgumentException>(() =>
            Prescricao.CriarEstruturada(
                PacienteId,
                Array.Empty<PrescricaoItem>()));
    }

    [Fact]
    public void CriarEstruturada_ComMedicamentoNaoCadastrado_DevePermitir()
    {
        // Arrange
        var item = CriarItemValido();

        // Act
        var prescricao = Prescricao.CriarEstruturada(
            PacienteId,
            new[] { item });

        // Assert
        Assert.Null(item.MedicamentoId);
        Assert.Equal("Medicamento manipulado", item.NomeMedicamento);
        Assert.Single(prescricao.Itens);
    }

    [Fact]
    public void AdicionarItem_EmPrescricaoNaoEstruturada_DeveLancarExcecao()
    {
        // Arrange
        var prescricao = Prescricao.CriarNaoEstruturada(
            PacienteId,
            "Prescrição em texto livre.");

        var item = CriarItemValido();

        // Act & Assert
        Assert.Throws<RegraNegocioException>(() =>
            prescricao.AdicionarItem(item));
    }

    [Fact]
    public void RemoverUltimoItem_DeveLancarExcecao()
    {
        // Arrange
        var item = CriarItemValido();

        var prescricao = Prescricao.CriarEstruturada(
            PacienteId,
            new[] { item });

        // Act & Assert
        Assert.Throws<RegraNegocioException>(() =>
            prescricao.RemoverItem(item.Id));

        Assert.Single(prescricao.Itens);
    }

    [Fact]
    public void AtualizarTextoLivre_EmPrescricaoEstruturada_DeveLancarExcecao()
    {
        var prescricao = Prescricao.CriarEstruturada(
            PacienteId,
            new[] { CriarItemValido() });

        Assert.Throws<RegraNegocioException>(() =>
            prescricao.AtualizarTextoLivre("Novo texto"));
    }

    private static PrescricaoItem CriarItemValido()
    {
        return new PrescricaoItem(
            medicamentoId: null,
            nomeMedicamento: "Medicamento manipulado",
            formaFarmaceutica: "Comprimido",
            dose: 500,
            unidadeMedida: "mg",
            frequencia: "A cada 8 horas",
            inicioTratamento: new DateOnly(2026, 11, 17),
            duracaoTratamentoDias: 7,
            usoContinuo: false,
            viaAdministracao: "Oral",
            prazoUtilizacao: "7 dias",
            observacao: "Administrar conforme orientação"
        );
    }
}
