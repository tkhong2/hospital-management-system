using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Outpatient.Domain.Enums;

namespace HospitalMS.Outpatient.Domain.Entities;

public class OutpatientServiceOrder : AuditableEntity
{
    public Guid EncounterId { get; set; }
    public OutpatientEncounter Encounter { get; set; } = null!;

    public Guid PatientId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public string? DoctorName { get; set; }
    public ServiceOrderStatus Status { get; set; } = ServiceOrderStatus.Ordered;
    public decimal TotalAmount { get; set; }
    public decimal TotalInsuranceAmount { get; set; }
    public decimal TotalPatientAmount { get; set; }

    public ICollection<OutpatientServiceOrderDetail> Details { get; set; } = new List<OutpatientServiceOrderDetail>();
}

public class OutpatientServiceOrderDetail : AuditableEntity
{
    public Guid ServiceOrderId { get; set; }
    public OutpatientServiceOrder ServiceOrder { get; set; } = null!;

    public Guid ServiceId { get; set; }
    public string ServiceCode { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string ServiceGroup { get; set; } = "XET_NGHIEM"; // XET_NGHIEM, CDHA, THAM_DO_CN, THU_THUAT
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal InsuranceCoverageRate { get; set; } = 0.8m; // Vd: 80% BHYT
    public decimal InsuranceCoveredAmount { get; set; }
    public decimal PatientPayAmount { get; set; }
    public string? RoomToExecute { get; set; }
    public string? ResultSummary { get; set; }
    public DateTime? ResultTime { get; set; }
    public bool IsAbnormal { get; set; }
}

