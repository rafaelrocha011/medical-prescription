using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.Abstractions.UseCases.Catalogos;
using MedicalPrescription.Application.DTOs.Catalogos;

namespace MedicalPrescription.Application.Queries.Catalogos;

public sealed class ListarPacientesUseCase : IListarPacientesUseCase
{
    private readonly IPacienteRepository _repository;

    public ListarPacientesUseCase(IPacienteRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<PacienteResumoDto>> ExecutarAsync(
        string? busca,
        CancellationToken cancellationToken = default)
    {
        var termo = busca?.Trim();

        if (termo?.Length > 100)
            throw new ArgumentException(
                "A pesquisa deve ter no maximo 100 caracteres.");

        return _repository.ListarAsync(
            string.IsNullOrWhiteSpace(termo) ? null : termo,
            cancellationToken);
    }
}
