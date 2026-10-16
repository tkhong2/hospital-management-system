using HospitalMS.BuildingBlocks.Common;
using HospitalMS.HealthExam.Domain.Enums;

namespace HospitalMS.HealthExam.Domain.Entities;

public class ReferenceRange : BaseEntity
{
    public string TestCode { get; set; } = string.Empty; // GLU, URE, CREAT, AST, ALT, CHOL...
    public string TestName { get; set; } = string.Empty;
    public TargetGender Gender { get; set; } = TargetGender.All;
    public int MinAge { get; set; } = 0;
    public int MaxAge { get; set; } = 150;
    public decimal? LowerLimit { get; set; } // Giới hạn dưới (VD: 3.9)
    public decimal? UpperLimit { get; set; } // Giới hạn trên (VD: 6.4)
    public string? NormalText { get; set; } // Dành cho chỉ số định tính (VD: "Âm tính")
    public string? Unit { get; set; } // mmol/L
    public string? Description { get; set; }
}

