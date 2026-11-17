using MedicalPrescription.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MySql;

namespace MedicalPrescription.IntegrationTests;

public sealed class MySqlFixture : IAsyncLifetime
{
    private readonly MySqlContainer _container =
        new MySqlBuilder("mysql:8.4").Build();

    public MedicalPrescriptionDbContext NovoContexto()
    {
        var options = new DbContextOptionsBuilder<MedicalPrescriptionDbContext>()
            .UseMySql(
                _container.GetConnectionString(),
                new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;

        return new MedicalPrescriptionDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var context = NovoContexto();

        // Aplica as migrations reais no banco temporario.
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}