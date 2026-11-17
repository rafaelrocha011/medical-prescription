using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Application.Abstractions.UseCases.Prescricoes;

namespace MedicalPrescription.Application.UseCases.Prescricoes.ExcluirPrescricao;

public sealed class ExcluirPrescricaoUseCase : IExcluirPrescricaoUseCase
{
    private readonly IPrescricaoRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ExcluirPrescricaoUseCase(
        IPrescricaoRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecutarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Prescricao invalida.");

        var prescricao = await _repository.ObterPorIdAsync(
            id, cancellationToken);

        if (prescricao is null)
            throw new KeyNotFoundException("Prescricao nao encontrada.");

        _repository.Remover(prescricao);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
