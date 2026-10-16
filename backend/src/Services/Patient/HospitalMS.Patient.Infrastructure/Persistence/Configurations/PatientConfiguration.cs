using HospitalMS.Patient.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalMS.Patient.Infrastructure.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Domain.Entities.Patient>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Patient> builder)
    {
        builder.ToTable("Patients");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.PatientCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(p => p.PatientCode)
            .IsUnique();

        builder.Property(p => p.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(p => p.FullName);

        builder.Property(p => p.IdentityCardNumber)
            .HasMaxLength(20);

        builder.HasIndex(p => p.IdentityCardNumber);

        builder.Property(p => p.HealthInsuranceNumber)
            .HasMaxLength(20);

        builder.HasIndex(p => p.HealthInsuranceNumber);

        builder.Property(p => p.PhoneNumber)
            .HasMaxLength(20);

        builder.HasIndex(p => p.PhoneNumber);

        builder.Property(p => p.Email)
            .HasMaxLength(150);

        builder.Property(p => p.DetailAddress)
            .HasMaxLength(300);

        builder.Property(p => p.ProvinceName)
            .HasMaxLength(100);

        builder.Property(p => p.DistrictName)
            .HasMaxLength(100);

        builder.Property(p => p.WardName)
            .HasMaxLength(100);

        builder.Property(p => p.Ethnic)
            .HasMaxLength(50);

        builder.Property(p => p.Nationality)
            .HasMaxLength(50);

        builder.Property(p => p.Career)
            .HasMaxLength(100);

        builder.Property(p => p.Allergies)
            .HasMaxLength(500);

        builder.Property(p => p.Note)
            .HasMaxLength(1000);

        builder.Property(p => p.MergeReason)
            .HasMaxLength(500);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}

public class PatientMergeHistoryConfiguration : IEntityTypeConfiguration<PatientMergeHistory>
{
    public void Configure(EntityTypeBuilder<PatientMergeHistory> builder)
    {
        builder.ToTable("PatientMergeHistories");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.SourcePatientCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(h => h.TargetPatientCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(h => h.MergeReason)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(h => h.MergedBy)
            .HasMaxLength(100);

        builder.HasIndex(h => h.SourcePatientId);
        builder.HasIndex(h => h.TargetPatientId);
    }
}

