using MedicalPrescription.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedicalPrescription.Infrastructure.Persistence.Configurations;

public sealed class PrescricaoItemConfiguration
    : IEntityTypeConfiguration<PrescricaoItem>
{
    public void Configure(EntityTypeBuilder<PrescricaoItem> builder)
    {
        builder.ToTable("prescricao_itens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(x => x.PrescricaoId)
            .HasColumnName("prescricao_id")
            .IsRequired();

        builder.Property(x => x.MedicamentoId)
            .HasColumnName("medicamento_id");

        builder.HasOne<Medicamento>()
            .WithMany()
            .HasForeignKey(x => x.MedicamentoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.NomeMedicamento)
            .HasColumnName("nome_medicamento")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.FormaFarmaceutica)
            .HasColumnName("forma_farmaceutica")
            .HasMaxLength(100)
            .IsRequired();

        builder.OwnsOne(x => x.Dosagem, dosagem =>
        {
            dosagem.Property(x => x.Quantidade)
                .HasColumnName("dose")
                .HasPrecision(12, 3)
                .IsRequired();

            dosagem.Property(x => x.UnidadeMedida)
                .HasColumnName("unidade_medida")
                .HasMaxLength(20)
                .IsRequired();
        });

        builder.Navigation(x => x.Dosagem).IsRequired();

        builder.Property(x => x.Frequencia)
            .HasColumnName("frequencia")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.InicioTratamento)
            .HasColumnName("inicio_tratamento")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.DuracaoTratamentoDias)
            .HasColumnName("duracao_tratamento_dias");

        builder.Property(x => x.UsoContinuo)
            .HasColumnName("uso_continuo")
            .IsRequired();

        builder.Property(x => x.ViaAdministracao)
            .HasColumnName("via_administracao")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PrazoUtilizacao)
            .HasColumnName("prazo_utilizacao")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Observacao)
            .HasColumnName("observacao")
            .HasColumnType("longtext");
    }
}
