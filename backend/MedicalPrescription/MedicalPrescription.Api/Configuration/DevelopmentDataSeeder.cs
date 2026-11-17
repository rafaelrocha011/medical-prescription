using MedicalPrescription.Domain.Entities;
using MedicalPrescription.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace MedicalPrescription.Api.Configuration;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<MedicalPrescriptionDbContext>();

        if (!await db.Pacientes.AnyAsync())
        {
            db.Pacientes.AddRange(
                new Paciente("Paciente Exemplo Um"),
                new Paciente("Paciente Exemplo Dois"));
        }

        if (!await db.Medicamentos.AnyAsync())
        {
            db.Medicamentos.AddRange(
                new Medicamento("Medicamento Exemplo A"),
                new Medicamento("Medicamento Exemplo B"));
        }

        await db.SaveChangesAsync();

        var pacientes = await db.Pacientes
            .AsNoTracking()
            .Select(p => new { p.Id, p.NomeCompleto })
            .ToListAsync();

        var medicamentos = await db.Medicamentos
            .AsNoTracking()
            .Select(m => new { m.Id, m.Nome })
            .ToListAsync();

        Console.WriteLine("=== PACIENTES DE DESENVOLVIMENTO ===");

        foreach (var paciente in pacientes)
            Console.WriteLine($"{paciente.Id} - {paciente.NomeCompleto}");

        Console.WriteLine("=== MEDICAMENTOS DE DESENVOLVIMENTO ===");

        foreach (var medicamento in medicamentos)
            Console.WriteLine($"{medicamento.Id} - {medicamento.Nome}");
    }
}
