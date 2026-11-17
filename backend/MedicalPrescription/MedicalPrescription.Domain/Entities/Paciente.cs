
namespace MedicalPrescription.Domain.Entities;

public class Paciente
{
    public Guid Id { get; private set; }
    public string NomeCompleto { get; private set; } = string.Empty;

    private Paciente() { }

    public Paciente(string nomeCompleto)
    {
        if (string.IsNullOrWhiteSpace(nomeCompleto))
            throw new ArgumentException(
                "O nome do paciente é obrigatório.");

        Id = Guid.NewGuid();
        NomeCompleto = nomeCompleto.Trim();
    }
}
