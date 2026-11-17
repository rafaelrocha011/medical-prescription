using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.Abstractions.UseCases.Prescricoes;
using MedicalPrescription.Application.DTOs.Prescricoes;

namespace MedicalPrescription.Application.Queries.Prescricoes.ObterPrescricao;

public sealed class ObterPrescricaoUseCase : IObterPrescricaoUseCase
{
    private readonly IPrescricaoReadRepository _repository;

    public ObterPrescricaoUseCase(IPrescricaoReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<PrescricaoDetalheDto> ExecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Prescricao invalida.");

        var resultado = await _repository.ObterPorIdAsync(
            id, cancellationToken);

        return resultado
            ?? throw new KeyNotFoundException(
                "Prescricao nao encontrada.");
    }
}