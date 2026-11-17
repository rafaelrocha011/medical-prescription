using MedicalPrescription.Application.Abstractions.UseCases.Prescricoes;
using MedicalPrescription.Application.UseCases.Prescricoes.CriarPrescricao;
using MedicalPrescription.Application.UseCases.Prescricoes.AtualizarPrescricao;
using MedicalPrescription.Application.UseCases.Prescricoes.ObterPrescricao;
using MedicalPrescription.Application.UseCases.Prescricoes.ListarPrescricoes;
using MedicalPrescription.Application.UseCases.Prescricoes.ExcluirPrescricao;
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

        return services;
    }
}
