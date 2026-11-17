using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Infrastructure.Persistence.Context;
using MedicalPrescription.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MedicalPrescription.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("MySql");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:MySql nao foi configurada.");
        }

        services.AddDbContext<MedicalPrescriptionDbContext>(
            options => options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 4, 0))));

        services.AddScoped<IUnitOfWork>(
            provider => provider.GetRequiredService<
                MedicalPrescriptionDbContext>());

        services.AddScoped<
            IPacienteRepository,
            PacienteRepository>();

        services.AddScoped<
            IMedicamentoRepository,
            MedicamentoRepository>();

        services.AddScoped<
            IPrescricaoWriteRepository,
            PrescricaoWriteRepository>();

        services.AddScoped<PrescricaoReadRepository>();

        services.AddScoped<IPrescricaoReadRepository>(
            sp => sp.GetRequiredService<PrescricaoReadRepository>());

        services.AddScoped<IPrescricaoODataConsultaRepository>(
            sp => sp.GetRequiredService<PrescricaoReadRepository>());
        return services;
    }
}

