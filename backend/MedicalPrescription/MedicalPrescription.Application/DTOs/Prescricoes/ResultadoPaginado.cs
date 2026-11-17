namespace MedicalPrescription.Application.DTOs.Prescricoes;

public sealed record ResultadoPaginado<T>(
    IReadOnlyList<T> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalRegistros)
{
    public int TotalPaginas =>
        TamanhoPagina > 0
            ? (int)Math.Ceiling(
                (double)TotalRegistros / TamanhoPagina)
            : 0;
}
