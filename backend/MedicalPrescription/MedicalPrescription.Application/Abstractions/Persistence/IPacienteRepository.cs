namespace MedicalPrescription.Application.Abstractions.Persistence;

public interface IPacienteRepository
{
    Task<bool> ExisteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
