
namespace MedicalPrescription.Domain.ValueObjects;

public sealed record Dosagem
{
    public const int PrecisaoTotal = 12;
    public const int CasasDecimais = 3;
    public const int TamanhoMaximoUnidade = 20;
    public const decimal QuantidadeMaxima = 999_999_999.999m;

    public decimal Quantidade { get; private set; }
    public string UnidadeMedida { get; private set; } = string.Empty;

    private Dosagem() { }

    public Dosagem(decimal quantidade, string unidadeMedida)
    {
        if (quantidade <= 0)
            throw new ArgumentException(
                "A quantidade deve ser maior que zero.",
                nameof(quantidade));

        if (quantidade > QuantidadeMaxima)
            throw new ArgumentException(
                "A quantidade excede o limite permitido.",
                nameof(quantidade));

        if (decimal.Round(quantidade, CasasDecimais) != quantidade)
            throw new ArgumentException(
                "A quantidade admite no maximo tres casas decimais.",
                nameof(quantidade));

        if (string.IsNullOrWhiteSpace(unidadeMedida))
            throw new ArgumentException(
                "A unidade de medida e obrigatoria.",
                nameof(unidadeMedida));

        var unidadeNormalizada = unidadeMedida.Trim();

        if (unidadeNormalizada.Length > TamanhoMaximoUnidade)
            throw new ArgumentException(
                "A unidade de medida excede o tamanho permitido.",
                nameof(unidadeMedida));

        Quantidade = quantidade;
        UnidadeMedida = unidadeNormalizada;
    }
}
