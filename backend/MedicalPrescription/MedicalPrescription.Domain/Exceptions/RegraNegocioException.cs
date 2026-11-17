namespace MedicalPrescription.Domain.Exceptions;

public sealed class RegraNegocioException : Exception
{
    public RegraNegocioException(string mensagem)
        : base(mensagem)
    {
    }

    public RegraNegocioException(
        string mensagem,
        Exception innerException)
        : base(mensagem, innerException)
    {
    }
}