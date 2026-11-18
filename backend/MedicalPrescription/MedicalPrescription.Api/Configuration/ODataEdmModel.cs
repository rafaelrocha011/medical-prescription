using MedicalPrescription.Application.DTOs.Prescricoes;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace MedicalPrescription.Api.Configuration;

public static class ODataEdmModel
{
    public static IEdmModel Criar()
    {
        var builder = new ODataConventionModelBuilder();

        builder.EntitySet<PrescricaoODataDto>(
            "ConsultaPrescricoes");

        return builder.GetEdmModel();
    }
}
