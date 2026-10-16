using HospitalMS.Inpatient.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Inpatient.Application.Common.Interfaces;

public interface IInpatientDbContext
{
    DbSet<DepartmentRoom> Rooms { get; }
    DbSet<HospitalBed> Beds { get; }
    DbSet<InpatientAdmission> Admissions { get; }
    DbSet<BedTransferHistory> TransferHistories { get; }
    DbSet<InpatientMedicalOrder> MedicalOrders { get; }
    DbSet<InpatientVitalSign> VitalSigns { get; }
    DbSet<InpatientCareNote> CareNotes { get; }
    DbSet<InpatientAdvancePayment> AdvancePayments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

