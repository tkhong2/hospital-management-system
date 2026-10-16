using HospitalMS.HealthExam.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalMS.HealthExam.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.TaxCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.RepresentativeName).HasMaxLength(150);
        builder.Property(x => x.PhoneNumber).HasMaxLength(50);
        builder.Property(x => x.Email).HasMaxLength(100);
        builder.Property(x => x.Address).HasMaxLength(500);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.TaxCode);
    }
}

public class ExamContractConfiguration : IEntityTypeConfiguration<ExamContract>
{
    public void Configure(EntityTypeBuilder<ExamContract> builder)
    {
        builder.ToTable("ExamContracts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ContractNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(250).IsRequired();
        builder.Property(x => x.LocationAddress).HasMaxLength(500);
        builder.Property(x => x.Note).HasMaxLength(1000);

        builder.Property(x => x.TotalContractAmount).HasPrecision(18, 2);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        builder.Property(x => x.FinalAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalPaidAmount).HasPrecision(18, 2);

        builder.HasIndex(x => x.ContractNumber).IsUnique();
        builder.HasIndex(x => x.CompanyId);
        builder.HasIndex(x => x.Status);

        builder.HasOne(x => x.Company)
            .WithMany(x => x.Contracts)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ContractPaymentConfiguration : IEntityTypeConfiguration<ContractPayment>
{
    public void Configure(EntityTypeBuilder<ContractPayment> builder)
    {
        builder.ToTable("ContractPayments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PaymentCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.TransactionReference).HasMaxLength(200);
        builder.Property(x => x.Note).HasMaxLength(500);

        builder.HasIndex(x => x.ContractId);
        builder.HasIndex(x => x.PaymentCode);

        builder.HasOne(x => x.Contract)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ExamPackageConfiguration : IEntityTypeConfiguration<ExamPackage>
{
    public void Configure(EntityTypeBuilder<ExamPackage> builder)
    {
        builder.ToTable("ExamPackages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PackageCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PackageName).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.BasePrice).HasPrecision(18, 2);
        builder.Property(x => x.PackagePrice).HasPrecision(18, 2);

        builder.HasIndex(x => x.PackageCode);
        builder.HasIndex(x => x.ContractId);

        builder.HasOne(x => x.Contract)
            .WithMany(x => x.Packages)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class PackageServiceConfiguration : IEntityTypeConfiguration<PackageService>
{
    public void Configure(EntityTypeBuilder<PackageService> builder)
    {
        builder.ToTable("PackageServices");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ServiceCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ServiceName).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Price).HasPrecision(18, 2);

        builder.HasIndex(x => x.PackageId);
        builder.HasIndex(x => x.MedicalServiceId);

        builder.HasOne(x => x.Package)
            .WithMany(x => x.PackageServices)
            .HasForeignKey(x => x.PackageId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ExamStation)
            .WithMany(x => x.PackageServices)
            .HasForeignKey(x => x.ExamStationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ExamStationConfiguration : IEntityTypeConfiguration<ExamStation>
{
    public void Configure(EntityTypeBuilder<ExamStation> builder)
    {
        builder.ToTable("ExamStations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.SpecialtyCode).HasMaxLength(50);
        builder.Property(x => x.RoomNumber).HasMaxLength(50);
        builder.Property(x => x.DefaultDoctorName).HasMaxLength(150);

        builder.HasIndex(x => x.Code).IsUnique();
    }
}

public class ExamRegistrationConfiguration : IEntityTypeConfiguration<ExamRegistration>
{
    public void Configure(EntityTypeBuilder<ExamRegistration> builder)
    {
        builder.ToTable("ExamRegistrations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RegistrationCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.IdentityCardNumber).HasMaxLength(50);
        builder.Property(x => x.PhoneNumber).HasMaxLength(50);
        builder.Property(x => x.EmployeeCode).HasMaxLength(50);
        builder.Property(x => x.DepartmentName).HasMaxLength(150);
        builder.Property(x => x.Position).HasMaxLength(100);
        builder.Property(x => x.DriverLicenseClass).HasMaxLength(20);

        builder.Property(x => x.ExtraServicesAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.PaidAmount).HasPrecision(18, 2);

        builder.HasIndex(x => x.RegistrationCode).IsUnique();
        builder.HasIndex(x => x.ContractId);
        builder.HasIndex(x => x.PatientId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.IdentityCardNumber);

        builder.HasOne(x => x.Contract)
            .WithMany(x => x.Registrations)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Package)
            .WithMany(x => x.Registrations)
            .HasForeignKey(x => x.PackageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ExamVisitConfiguration : IEntityTypeConfiguration<ExamVisit>
{
    public void Configure(EntityTypeBuilder<ExamVisit> builder)
    {
        builder.ToTable("ExamVisits");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.VisitCode).HasMaxLength(100).IsRequired();

        builder.Property(x => x.HeightCm).HasPrecision(5, 2);
        builder.Property(x => x.WeightKg).HasPrecision(5, 2);
        builder.Property(x => x.Bmi).HasPrecision(4, 2);

        builder.HasIndex(x => x.VisitCode).IsUnique();
        builder.HasIndex(x => x.RegistrationId);
        builder.HasIndex(x => x.Status);

        builder.HasOne(x => x.Registration)
            .WithMany(x => x.Visits)
            .HasForeignKey(x => x.RegistrationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CurrentStation)
            .WithMany()
            .HasForeignKey(x => x.CurrentStationId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class StationResultConfiguration : IEntityTypeConfiguration<StationResult>
{
    public void Configure(EntityTypeBuilder<StationResult> builder)
    {
        builder.ToTable("StationResults");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DoctorName).HasMaxLength(150);
        builder.Property(x => x.ClinicalConclusion).HasMaxLength(1000);
        builder.Property(x => x.Notes).HasMaxLength(500);

        builder.HasIndex(x => new { x.VisitId, x.StationId }).IsUnique();

        builder.HasOne(x => x.Visit)
            .WithMany(x => x.StationResults)
            .HasForeignKey(x => x.VisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Station)
            .WithMany(x => x.StationResults)
            .HasForeignKey(x => x.StationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class LabOrderConfiguration : IEntityTypeConfiguration<LabOrder>
{
    public void Configure(EntityTypeBuilder<LabOrder> builder)
    {
        builder.ToTable("LabOrders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OrderCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.SampleCode).HasMaxLength(100);
        builder.Property(x => x.OrderedByDoctorName).HasMaxLength(150);

        builder.HasIndex(x => x.OrderCode);
        builder.HasIndex(x => x.VisitId);
        builder.HasIndex(x => x.Status);

        builder.HasOne(x => x.Visit)
            .WithMany(x => x.LabOrders)
            .HasForeignKey(x => x.VisitId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LabResultConfiguration : IEntityTypeConfiguration<LabResult>
{
    public void Configure(EntityTypeBuilder<LabResult> builder)
    {
        builder.ToTable("LabResults");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TestCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.TestName).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(250);
        builder.Property(x => x.NumericValue).HasPrecision(18, 4);
        builder.Property(x => x.Unit).HasMaxLength(50);
        builder.Property(x => x.NormalRangeText).HasMaxLength(100);
        builder.Property(x => x.PerformedBy).HasMaxLength(150);

        builder.HasIndex(x => x.LabOrderId);
        builder.HasIndex(x => x.TestCode);
        builder.HasIndex(x => x.IsAbnormal);

        builder.HasOne(x => x.LabOrder)
            .WithMany(x => x.Results)
            .HasForeignKey(x => x.LabOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ReferenceRange)
            .WithMany()
            .HasForeignKey(x => x.ReferenceRangeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ReferenceRangeConfiguration : IEntityTypeConfiguration<ReferenceRange>
{
    public void Configure(EntityTypeBuilder<ReferenceRange> builder)
    {
        builder.ToTable("ReferenceRanges");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TestCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.TestName).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Unit).HasMaxLength(50);
        builder.Property(x => x.LowerLimit).HasPrecision(18, 4);
        builder.Property(x => x.UpperLimit).HasPrecision(18, 4);
        builder.Property(x => x.NormalText).HasMaxLength(100);

        builder.HasIndex(x => new { x.TestCode, x.Gender });
    }
}

public class ExamConclusionConfiguration : IEntityTypeConfiguration<ExamConclusion>
{
    public void Configure(EntityTypeBuilder<ExamConclusion> builder)
    {
        builder.ToTable("ExamConclusions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ConcludingDoctorName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.DoctorConclusionText).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.MainDiseaseIcd10Code).HasMaxLength(20);
        builder.Property(x => x.MainDiseaseName).HasMaxLength(250);
        builder.Property(x => x.HealthAdvice).HasMaxLength(2000);
        builder.Property(x => x.ConsultationNotes).HasMaxLength(2000);
        builder.Property(x => x.ApprovedBy).HasMaxLength(150);

        builder.HasIndex(x => x.VisitId).IsUnique();

        builder.HasOne(x => x.Visit)
            .WithOne(x => x.Conclusion)
            .HasForeignKey<ExamConclusion>(x => x.VisitId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class HealthCertificateConfiguration : IEntityTypeConfiguration<HealthCertificate>
{
    public void Configure(EntityTypeBuilder<HealthCertificate> builder)
    {
        builder.ToTable("HealthCertificates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CertificateNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.IssuedByHospitalName).HasMaxLength(250).IsRequired();
        builder.Property(x => x.DirectorSignatureName).HasMaxLength(150);
        builder.Property(x => x.QrVerifyCode).HasMaxLength(200).IsRequired();
        builder.Property(x => x.PdfUrl).HasMaxLength(500);

        builder.HasIndex(x => x.CertificateNumber).IsUnique();
        builder.HasIndex(x => x.QrVerifyCode).IsUnique();
        builder.HasIndex(x => x.VisitId).IsUnique();

        builder.HasOne(x => x.Visit)
            .WithOne(x => x.Certificate)
            .HasForeignKey<HealthCertificate>(x => x.VisitId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

