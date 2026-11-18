using MedicalPrescription.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalPrescription.Infrastructure.Persistence.Configurations;

public sealed class PrescricaoConfiguration
    : IEntityTypeConfiguration<Prescricao>
{
    public void Configure(EntityTypeBuilder<Prescricao> builder)
    {
        builder.ToTable("prescricoes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.PacienteId)
            .HasColumnName("paciente_id")
            .IsRequired();

        builder.Property(x => x.Tipo)
            .HasColumnName("tipo")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.TextoLivre)
            .HasColumnName("texto_livre")
            .HasColumnType("longtext");

        builder.Property(x => x.CriadaEmUtc)
            .HasColumnName("criada_em_utc")
            .HasColumnType("datetime(6)")
            .HasConversion(
                value => value,
                value => DateTime.SpecifyKind(
                    value, DateTimeKind.Utc))
            .IsRequired();

        builder.HasOne<Paciente>()
            .WithMany()
            .HasForeignKey(x => x.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Itens)
            .WithOne()
            .HasForeignKey(x => x.PrescricaoId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Itens)
            .HasField("_itens")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => x.CriadaEmUtc);
    }
}
