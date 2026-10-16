using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Outpatient.Domain.Enums;

namespace HospitalMS.Outpatient.Domain.Entities;

public class OutpatientPrescription : AuditableEntity
{
    public Guid EncounterId { get; set; }
    public OutpatientEncounter Encounter { get; set; } = null!;

    public Guid PatientId { get; set; }
    public string PrescriptionCode { get; set; } = string.Empty;
    public DateTime PrescriptionDate { get; set; } = DateTime.UtcNow;
    public string? DoctorName { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public int DaysCount { get; set; } = 5;
    public string? Advice { get; set; }
    public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Prescribed;
    public decimal TotalAmount { get; set; }
    public decimal TotalInsuranceAmount { get; set; }
    public decimal TotalPatientAmount { get; set; }

    public ICollection<OutpatientPrescriptionItem> Items { get; set; } = new List<OutpatientPrescriptionItem>();
}

public class OutpatientPrescriptionItem : AuditableEntity
{
    public Guid PrescriptionId { get; set; }
    public OutpatientPrescription Prescription { get; set; } = null!;

    public Guid MedicineId { get; set; }
    public string MedicineCode { get; set; } = string.Empty;
    public string MedicineName { get; set; } = string.Empty;
    public string? ActiveIngredient { get; set; }
    public string? DosageForm { get; set; } // Viên, Gói, Chai
    public int Quantity { get; set; }
    public string? MorningDose { get; set; }
    public string? NoonDose { get; set; }
    public string? AfternoonDose { get; set; }
    public string? NightDose { get; set; }
    public string? UsageInstructions { get; set; } // Vd: Uống sau ăn no
    public decimal UnitPrice { get; set; }
    public bool IsInsuranceCovered { get; set; } = true;
    public decimal InsuranceCoveredAmount { get; set; }
    public decimal PatientPayAmount { get; set; }
}

