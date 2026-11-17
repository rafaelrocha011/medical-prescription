
namespace MedicalPrescription.Domain.Entities;

public class Medicamento
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;

    private Medicamento() { }

    public Medicamento(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException(
                "O nome do medicamento é obrigatório.");

        Id = Guid.NewGuid();
        Nome = nome.Trim();
    }
}
