using MedicalPrescription.Application.Abstractions.Persistence;
using MedicalPrescription.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicalPrescription.Infrastructure.Persistence.Context;

public sealed class MedicalPrescriptionDbContext
    : DbContext, IUnitOfWork
{
    public MedicalPrescriptionDbContext(
        DbContextOptions<MedicalPrescriptionDbContext> options)
        : base(options)
    {
    }

    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Medicamento> Medicamentos => Set<Medicamento>();
    public DbSet<Prescricao> Prescricoes => Set<Prescricao>();
    public DbSet<PrescricaoItem> PrescricaoItens => Set<PrescricaoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(MedicalPrescriptionDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
