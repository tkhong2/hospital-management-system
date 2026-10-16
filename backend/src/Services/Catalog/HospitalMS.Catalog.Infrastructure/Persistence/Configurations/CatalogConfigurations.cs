using HospitalMS.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalMS.Catalog.Infrastructure.Persistence.Configurations;

public class MedicalServiceConfiguration : IEntityTypeConfiguration<MedicalService>
{
    public void Configure(EntityTypeBuilder<MedicalService> builder)
    {
        builder.ToTable("MedicalServices");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.Code)
            .IsUnique();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(s => s.Price)
            .HasPrecision(18, 2);

        builder.Property(s => s.HealthInsurancePrice)
            .HasPrecision(18, 2);

        builder.HasIndex(s => s.ServiceType);
        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}

public class Icd10Configuration : IEntityTypeConfiguration<Icd10>
{
    public void Configure(EntityTypeBuilder<Icd10> builder)
    {
        builder.ToTable("Icd10s");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(i => i.Code)
            .IsUnique();

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.HasIndex(i => i.ChapterCode);
        builder.HasQueryFilter(i => !i.IsDeleted);
    }
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(d => d.Code)
            .IsUnique();

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}

public class SpecialtyConfiguration : IEntityTypeConfiguration<Specialty>
{
    public void Configure(EntityTypeBuilder<Specialty> builder)
    {
        builder.ToTable("Specialties");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(s => s.Code)
            .IsUnique();

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("Units");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(u => u.Code)
            .IsUnique();

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}

public class AdministrativeDivisionConfiguration : IEntityTypeConfiguration<AdministrativeDivision>
{
    public void Configure(EntityTypeBuilder<AdministrativeDivision> builder)
    {
        builder.ToTable("AdministrativeDivisions");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(a => a.Code);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(a => new { a.Level, a.ParentCode });
    }
}

