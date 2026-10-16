using HospitalMS.Inpatient.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalMS.Inpatient.Infrastructure.Persistence.Configurations;

public class DepartmentRoomConfiguration : IEntityTypeConfiguration<DepartmentRoom>
{
    public void Configure(EntityTypeBuilder<DepartmentRoom> builder)
    {
        builder.ToTable("department_rooms");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DepartmentCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DepartmentName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.RoomNumber).HasMaxLength(20).IsRequired();
        builder.Property(x => x.RoomName).HasMaxLength(150).IsRequired();

        builder.HasMany(x => x.Beds)
            .WithOne(x => x.Room)
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class HospitalBedConfiguration : IEntityTypeConfiguration<HospitalBed>
{
    public void Configure(EntityTypeBuilder<HospitalBed> builder)
    {
        builder.ToTable("hospital_beds");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BedCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.BedName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DailyPrice).HasPrecision(18, 2);
        builder.Property(x => x.InsuranceCoveredPrice).HasPrecision(18, 2);

        builder.HasOne(x => x.CurrentAdmission)
            .WithMany()
            .HasForeignKey(x => x.CurrentAdmissionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class InpatientAdmissionConfiguration : IEntityTypeConfiguration<InpatientAdmission>
{
    public void Configure(EntityTypeBuilder<InpatientAdmission> builder)
    {
        builder.ToTable("admissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AdmissionCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PatientCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PatientName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TotalAdvancePayment).HasPrecision(18, 2);
        builder.Property(x => x.TotalInpatientExpenses).HasPrecision(18, 2);

        builder.HasMany(x => x.TransferHistories)
            .WithOne(x => x.Admission)
            .HasForeignKey(x => x.AdmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.MedicalOrders)
            .WithOne(x => x.Admission)
            .HasForeignKey(x => x.AdmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.VitalSigns)
            .WithOne(x => x.Admission)
            .HasForeignKey(x => x.AdmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.CareNotes)
            .WithOne(x => x.Admission)
            .HasForeignKey(x => x.AdmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.AdvancePayments)
            .WithOne(x => x.Admission)
            .HasForeignKey(x => x.AdmissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class InpatientVitalSignConfiguration : IEntityTypeConfiguration<InpatientVitalSign>
{
    public void Configure(EntityTypeBuilder<InpatientVitalSign> builder)
    {
        builder.ToTable("vital_signs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Pulse).HasPrecision(5, 2);
        builder.Property(x => x.Temperature).HasPrecision(4, 2);
        builder.Property(x => x.BloodPressureSystolic).HasPrecision(5, 2);
        builder.Property(x => x.BloodPressureDiastolic).HasPrecision(5, 2);
        builder.Property(x => x.BreathingRate).HasPrecision(4, 2);
        builder.Property(x => x.SpO2).HasPrecision(5, 2);
        builder.Property(x => x.Weight).HasPrecision(5, 2);
    }
}

public class InpatientAdvancePaymentConfiguration : IEntityTypeConfiguration<InpatientAdvancePayment>
{
    public void Configure(EntityTypeBuilder<InpatientAdvancePayment> builder)
    {
        builder.ToTable("advance_payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
    }
}

