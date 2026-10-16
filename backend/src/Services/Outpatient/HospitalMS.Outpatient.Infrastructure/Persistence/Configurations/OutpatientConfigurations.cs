using HospitalMS.Outpatient.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalMS.Outpatient.Infrastructure.Persistence.Configurations;

public class OutpatientReceptionConfiguration : IEntityTypeConfiguration<OutpatientReception>
{
    public void Configure(EntityTypeBuilder<OutpatientReception> builder)
    {
        builder.ToTable("receptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PatientCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PatientName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ClinicRoomName).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => new { x.ClinicRoomId, x.ReceptionTime });
    }
}

public class OutpatientEncounterConfiguration : IEntityTypeConfiguration<OutpatientEncounter>
{
    public void Configure(EntityTypeBuilder<OutpatientEncounter> builder)
    {
        builder.ToTable("encounters");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.Reception)
            .WithOne(x => x.Encounter)
            .HasForeignKey<OutpatientEncounter>(x => x.ReceptionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.Pulse).HasPrecision(5, 2);
        builder.Property(x => x.BloodPressureSystolic).HasPrecision(5, 2);
        builder.Property(x => x.BloodPressureDiastolic).HasPrecision(5, 2);
        builder.Property(x => x.Temperature).HasPrecision(4, 2);
        builder.Property(x => x.SpO2).HasPrecision(5, 2);
        builder.Property(x => x.Weight).HasPrecision(5, 2);
        builder.Property(x => x.Height).HasPrecision(5, 2);
    }
}

public class OutpatientServiceOrderConfiguration : IEntityTypeConfiguration<OutpatientServiceOrder>
{
    public void Configure(EntityTypeBuilder<OutpatientServiceOrder> builder)
    {
        builder.ToTable("service_orders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalInsuranceAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalPatientAmount).HasPrecision(18, 2);

        builder.HasOne(x => x.Encounter)
            .WithMany(x => x.ServiceOrders)
            .HasForeignKey(x => x.EncounterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OutpatientServiceOrderDetailConfiguration : IEntityTypeConfiguration<OutpatientServiceOrderDetail>
{
    public void Configure(EntityTypeBuilder<OutpatientServiceOrderDetail> builder)
    {
        builder.ToTable("service_order_details");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.InsuranceCoverageRate).HasPrecision(5, 2);
        builder.Property(x => x.InsuranceCoveredAmount).HasPrecision(18, 2);
        builder.Property(x => x.PatientPayAmount).HasPrecision(18, 2);
    }
}

public class OutpatientPrescriptionConfiguration : IEntityTypeConfiguration<OutpatientPrescription>
{
    public void Configure(EntityTypeBuilder<OutpatientPrescription> builder)
    {
        builder.ToTable("prescriptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalInsuranceAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalPatientAmount).HasPrecision(18, 2);

        builder.HasOne(x => x.Encounter)
            .WithMany(x => x.Prescriptions)
            .HasForeignKey(x => x.EncounterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OutpatientPrescriptionItemConfiguration : IEntityTypeConfiguration<OutpatientPrescriptionItem>
{
    public void Configure(EntityTypeBuilder<OutpatientPrescriptionItem> builder)
    {
        builder.ToTable("prescription_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.InsuranceCoveredAmount).HasPrecision(18, 2);
        builder.Property(x => x.PatientPayAmount).HasPrecision(18, 2);
    }
}

public class OutpatientBillingConfiguration : IEntityTypeConfiguration<OutpatientBilling>
{
    public void Configure(EntityTypeBuilder<OutpatientBilling> builder)
    {
        builder.ToTable("billings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.InsuranceCoveredAmount).HasPrecision(18, 2);
        builder.Property(x => x.PatientPayAmount).HasPrecision(18, 2);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        builder.Property(x => x.AmountPaid).HasPrecision(18, 2);

        builder.HasOne(x => x.Encounter)
            .WithMany(x => x.Billings)
            .HasForeignKey(x => x.EncounterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OutpatientBillingItemConfiguration : IEntityTypeConfiguration<OutpatientBillingItem>
{
    public void Configure(EntityTypeBuilder<OutpatientBillingItem> builder)
    {
        builder.ToTable("billing_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.InsuranceCoveredAmount).HasPrecision(18, 2);
        builder.Property(x => x.PatientPayAmount).HasPrecision(18, 2);
    }
}

