using HospitalMS.Outpatient.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Outpatient.Application.Common.Interfaces;

public interface IOutpatientDbContext
{
    DbSet<OutpatientReception> Receptions { get; }
    DbSet<OutpatientEncounter> Encounters { get; }
    DbSet<OutpatientServiceOrder> ServiceOrders { get; }
    DbSet<OutpatientServiceOrderDetail> ServiceOrderDetails { get; }
    DbSet<OutpatientPrescription> Prescriptions { get; }
    DbSet<OutpatientPrescriptionItem> PrescriptionItems { get; }
    DbSet<OutpatientBilling> Billings { get; }
    DbSet<OutpatientBillingItem> BillingItems { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

