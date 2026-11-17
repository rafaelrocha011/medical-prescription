namespace MedicalPrescription.Application.DTOs.Prescricoes;

public sealed class PrescricaoODataDto
{
    public Guid Id { get; set; }
    public Guid PacienteId { get; set; }
    public int Tipo { get; set; }
    public DateTime CriadaEmUtc { get; set; }
    public int QuantidadeMedicamentos { get; set; }
}
