
namespace MedicalPrescription.Domain.ValueObjects;

public sealed record Dosagem
{
    public decimal Quantidade { get; private set; }
    public string UnidadeMedida { get; private set; } = string.Empty;

    private Dosagem() { }

    public Dosagem(decimal quantidade, string unidadeMedida)
    {
        if (quantidade <= 0)
            throw new ArgumentException(
                "A quantidade deve ser maior que zero.",
                nameof(quantidade));

        if (string.IsNullOrWhiteSpace(unidadeMedida))
            throw new ArgumentException(
                "A unidade de medida é obrigatória.",
                nameof(unidadeMedida));

        Quantidade = quantidade;
        UnidadeMedida = unidadeMedida.Trim();
    }
}
