using HospitalMS.BuildingBlocks.Common;

namespace HospitalMS.Patient.Domain.Entities;

public class PatientMergeHistory : BaseEntity
{
    public Guid SourcePatientId { get; set; }
    public string SourcePatientCode { get; set; } = string.Empty;
    public Guid TargetPatientId { get; set; }
    public string TargetPatientCode { get; set; } = string.Empty;
    public string MergeReason { get; set; } = string.Empty;
    public string? MergedBy { get; set; }
    public DateTime MergedAt { get; set; } = DateTime.UtcNow;
    public string? SnapshotDataJson { get; set; }
}

