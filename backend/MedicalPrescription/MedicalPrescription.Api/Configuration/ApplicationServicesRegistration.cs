using MedicalPrescription.Application.Abstractions.UseCases.Catalogos;
using MedicalPrescription.Application.Queries.Catalogos;
using MedicalPrescription.Application.Abstractions.UseCases.Prescricoes;
using MedicalPrescription.Application.Commands.Prescricoes.CriarPrescricao;
using MedicalPrescription.Application.Commands.Prescricoes.AtualizarPrescricao;
using MedicalPrescription.Application.Queries.Prescricoes.ObterPrescricao;
using MedicalPrescription.Application.Queries.Prescricoes.ListarPrescricoes;
using MedicalPrescription.Application.Commands.Prescricoes.ExcluirPrescricao;
using Microsoft.Extensions.DependencyInjection;

namespace MedicalPrescription.Api.Configuration;

public static class ApplicationServicesRegistration
{
    public static IServiceCollection AddApplicationUseCases(
        this IServiceCollection services)
    {
        services.AddScoped<
            ICriarPrescricaoUseCase,
            CriarPrescricaoUseCase>();

        services.AddScoped<
            IAtualizarPrescricaoUseCase,
            AtualizarPrescricaoUseCase>();

        services.AddScoped<
            IObterPrescricaoUseCase,
            ObterPrescricaoUseCase>();

        services.AddScoped<
            IListarPrescricoesUseCase,
            ListarPrescricoesUseCase>();

        services.AddScoped<
            IExcluirPrescricaoUseCase,
            ExcluirPrescricaoUseCase>();

        services.AddScoped<IListarPacientesUseCase, ListarPacientesUseCase>();
        services.AddScoped<IListarMedicamentosUseCase, ListarMedicamentosUseCase>();
        return services;
    }
}

