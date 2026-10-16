using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Patient.Domain.Enums;

namespace HospitalMS.Patient.Domain.Entities;

public class Patient : AggregateRoot
{
    public string PatientCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; } = Gender.Male;
    public string? IdentityCardNumber { get; set; } // CCCD / CMND
    public string? HealthInsuranceNumber { get; set; } // Thẻ BHYT (15 ký tự)
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }

    // Address
    public string? DetailAddress { get; set; }
    public string? WardCode { get; set; }
    public string? WardName { get; set; }
    public string? DistrictCode { get; set; }
    public string? DistrictName { get; set; }
    public string? ProvinceCode { get; set; }
    public string? ProvinceName { get; set; }

    public string FullAddress
    {
        get
        {
            var parts = new[] { DetailAddress, WardName, DistrictName, ProvinceName }
                .Where(p => !string.IsNullOrWhiteSpace(p));
            return string.Join(", ", parts);
        }
    }

    // Demographics & Medical Profile
    public string? Ethnic { get; set; } = "Kinh";
    public string? Nationality { get; set; } = "Việt Nam";
    public string? Career { get; set; }
    public BloodType BloodType { get; set; } = BloodType.Unknown;
    public string? Allergies { get; set; }
    public string? Note { get; set; }

    // Patient Merge Management
    public bool IsMerged { get; set; } = false;
    public Guid? MergedToPatientId { get; set; }
    public string? MergeReason { get; set; }
    public DateTime? MergedAt { get; set; }

    public void MarkAsMerged(Guid targetPatientId, string reason)
    {
        IsMerged = true;
        MergedToPatientId = targetPatientId;
        MergeReason = reason;
        MergedAt = DateTime.UtcNow;
    }
}

