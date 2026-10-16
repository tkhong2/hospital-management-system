using FluentValidation;
using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Outpatient.Application.Common.Interfaces;
using HospitalMS.Outpatient.Application.DTOs;
using HospitalMS.Outpatient.Domain.Entities;
using HospitalMS.Outpatient.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Outpatient.Application.Features.Orders;

public record CreateServiceOrderCommand(
    Guid EncounterId,
    string? DoctorName,
    List<CreateServiceItemRequest> Items
) : IRequest<Result<ServiceOrderDto>>;

public record CreateServiceItemRequest(
    Guid ServiceId,
    string ServiceCode,
    string ServiceName,
    string ServiceGroup,
    int Quantity,
    decimal UnitPrice,
    decimal InsuranceCoverageRate,
    string? RoomToExecute
);

public class CreateServiceOrderHandler(IOutpatientDbContext dbContext)
    : IRequestHandler<CreateServiceOrderCommand, Result<ServiceOrderDto>>
{
    public async Task<Result<ServiceOrderDto>> Handle(CreateServiceOrderCommand request, CancellationToken cancellationToken)
    {
        var encounter = await dbContext.Encounters
            .Include(e => e.Reception)
            .FirstOrDefaultAsync(e => e.Id == request.EncounterId, cancellationToken);

        if (encounter == null)
            return Result.Failure<ServiceOrderDto>("Không tìm thấy phiên khám");

        var orderCode = $"CLS-{DateTime.UtcNow:yyMMdd}-{Random.Shared.Next(1000, 9999)}";
        var order = new OutpatientServiceOrder
        {
            EncounterId = encounter.Id,
            PatientId = encounter.PatientId,
            OrderCode = orderCode,
            OrderDate = DateTime.UtcNow,
            DoctorName = request.DoctorName ?? encounter.DoctorName,
            Status = ServiceOrderStatus.Ordered
        };

        decimal total = 0;
        decimal totalIns = 0;
        decimal totalPat = 0;

        foreach (var item in request.Items)
        {
            var lineTotal = item.UnitPrice * item.Quantity;
            var insAmount = lineTotal * item.InsuranceCoverageRate;
            var patAmount = lineTotal - insAmount;

            total += lineTotal;
            totalIns += insAmount;
            totalPat += patAmount;

            order.Details.Add(new OutpatientServiceOrderDetail
            {
                ServiceId = item.ServiceId,
                ServiceCode = item.ServiceCode,
                ServiceName = item.ServiceName,
                ServiceGroup = item.ServiceGroup,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                InsuranceCoverageRate = item.InsuranceCoverageRate,
                InsuranceCoveredAmount = insAmount,
                PatientPayAmount = patAmount,
                RoomToExecute = item.RoomToExecute
            });
        }

        order.TotalAmount = total;
        order.TotalInsuranceAmount = totalIns;
        order.TotalPatientAmount = totalPat;

        // Cập nhật trạng thái tiếp đón nếu cần chờ kết quả CLS
        if (encounter.Reception != null)
        {
            encounter.Reception.Status = ReceptionStatus.WaitingForLab;
        }

        dbContext.ServiceOrders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = new ServiceOrderDto(
            order.Id,
            order.EncounterId,
            order.OrderCode,
            order.OrderDate,
            order.DoctorName,
            order.Status,
            order.TotalAmount,
            order.TotalInsuranceAmount,
            order.TotalPatientAmount,
            order.Details.Select(d => new ServiceOrderDetailDto(
                d.Id, d.ServiceId, d.ServiceCode, d.ServiceName, d.ServiceGroup,
                d.Quantity, d.UnitPrice, d.InsuranceCoverageRate, d.InsuranceCoveredAmount,
                d.PatientPayAmount, d.RoomToExecute, d.ResultSummary, d.ResultTime, d.IsAbnormal
            )).ToList()
        );

        return Result.Success(dto);
    }
}

public record CreatePrescriptionCommand(
    Guid EncounterId,
    string? DoctorName,
    string Diagnosis,
    int DaysCount,
    string? Advice,
    List<CreatePrescriptionItemRequest> Items
) : IRequest<Result<PrescriptionDto>>;

public record CreatePrescriptionItemRequest(
    Guid MedicineId,
    string MedicineCode,
    string MedicineName,
    string? ActiveIngredient,
    string? DosageForm,
    int Quantity,
    string? MorningDose,
    string? NoonDose,
    string? AfternoonDose,
    string? NightDose,
    string? UsageInstructions,
    decimal UnitPrice,
    bool IsInsuranceCovered
);

public class CreatePrescriptionHandler(IOutpatientDbContext dbContext)
    : IRequestHandler<CreatePrescriptionCommand, Result<PrescriptionDto>>
{
    public async Task<Result<PrescriptionDto>> Handle(CreatePrescriptionCommand request, CancellationToken cancellationToken)
    {
        var encounter = await dbContext.Encounters.FirstOrDefaultAsync(e => e.Id == request.EncounterId, cancellationToken);
        if (encounter == null)
            return Result.Failure<PrescriptionDto>("Không tìm thấy phiên khám");

        var prescriptionCode = $"TOA-{DateTime.UtcNow:yyMMdd}-{Random.Shared.Next(1000, 9999)}";
        var prescription = new OutpatientPrescription
        {
            EncounterId = encounter.Id,
            PatientId = encounter.PatientId,
            PrescriptionCode = prescriptionCode,
            PrescriptionDate = DateTime.UtcNow,
            DoctorName = request.DoctorName ?? encounter.DoctorName,
            Diagnosis = string.IsNullOrWhiteSpace(request.Diagnosis) ? encounter.PrimaryIcd10Name : request.Diagnosis,
            DaysCount = request.DaysCount > 0 ? request.DaysCount : 5,
            Advice = request.Advice,
            Status = PrescriptionStatus.Prescribed
        };

        decimal total = 0;
        decimal totalIns = 0;
        decimal totalPat = 0;

        foreach (var item in request.Items)
        {
            var lineTotal = item.UnitPrice * item.Quantity;
            var insAmount = item.IsInsuranceCovered ? (lineTotal * 0.8m) : 0m;
            var patAmount = lineTotal - insAmount;

            total += lineTotal;
            totalIns += insAmount;
            totalPat += patAmount;

            prescription.Items.Add(new OutpatientPrescriptionItem
            {
                MedicineId = item.MedicineId,
                MedicineCode = item.MedicineCode,
                MedicineName = item.MedicineName,
                ActiveIngredient = item.ActiveIngredient,
                DosageForm = item.DosageForm,
                Quantity = item.Quantity,
                MorningDose = item.MorningDose,
                NoonDose = item.NoonDose,
                AfternoonDose = item.AfternoonDose,
                NightDose = item.NightDose,
                UsageInstructions = item.UsageInstructions,
                UnitPrice = item.UnitPrice,
                IsInsuranceCovered = item.IsInsuranceCovered,
                InsuranceCoveredAmount = insAmount,
                PatientPayAmount = patAmount
            });
        }

        prescription.TotalAmount = total;
        prescription.TotalInsuranceAmount = totalIns;
        prescription.TotalPatientAmount = totalPat;

        dbContext.Prescriptions.Add(prescription);
        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = new PrescriptionDto(
            prescription.Id,
            prescription.EncounterId,
            prescription.PrescriptionCode,
            prescription.PrescriptionDate,
            prescription.DoctorName,
            prescription.Diagnosis,
            prescription.DaysCount,
            prescription.Advice,
            prescription.Status,
            prescription.TotalAmount,
            prescription.TotalInsuranceAmount,
            prescription.TotalPatientAmount,
            prescription.Items.Select(i => new PrescriptionItemDto(
                i.Id, i.MedicineId, i.MedicineCode, i.MedicineName, i.ActiveIngredient,
                i.DosageForm, i.Quantity, i.MorningDose, i.NoonDose, i.AfternoonDose,
                i.NightDose, i.UsageInstructions, i.UnitPrice, i.IsInsuranceCovered,
                i.InsuranceCoveredAmount, i.PatientPayAmount
            )).ToList()
        );

        return Result.Success(dto);
    }
}

public record ProcessOutpatientBillingCommand(
    Guid EncounterId,
    string PaymentMethod,
    string? CashierName
) : IRequest<Result<BillingDto>>;

public class ProcessOutpatientBillingHandler(IOutpatientDbContext dbContext)
    : IRequestHandler<ProcessOutpatientBillingCommand, Result<BillingDto>>
{
    public async Task<Result<BillingDto>> Handle(ProcessOutpatientBillingCommand request, CancellationToken cancellationToken)
    {
        var encounter = await dbContext.Encounters
            .Include(e => e.ServiceOrders).ThenInclude(s => s.Details)
            .Include(e => e.Prescriptions).ThenInclude(p => p.Items)
            .FirstOrDefaultAsync(e => e.Id == request.EncounterId, cancellationToken);

        if (encounter == null)
            return Result.Failure<BillingDto>("Không tìm thấy phiên khám");

        var invoiceNum = $"INV-{DateTime.UtcNow:yyMMdd}-{Random.Shared.Next(1000, 9999)}";
        var billing = new OutpatientBilling
        {
            EncounterId = encounter.Id,
            PatientId = encounter.PatientId,
            InvoiceNumber = invoiceNum,
            InvoiceDate = DateTime.UtcNow,
            PaymentMethod = request.PaymentMethod,
            CashierName = request.CashierName ?? "Thu ngân 01",
            Status = BillingStatus.Paid
        };

        decimal total = 0;
        decimal totalIns = 0;
        decimal totalPat = 0;

        // Thêm phí khám ban đầu
        decimal examFee = 150000m;
        decimal examIns = 120000m;
        decimal examPat = examFee - examIns;
        total += examFee; totalIns += examIns; totalPat += examPat;

        billing.Items.Add(new OutpatientBillingItem
        {
            ItemType = "EXAM_FEE",
            ItemReferenceId = encounter.Id,
            ItemCode = "KB-01",
            ItemName = "Tiền khám chuyên khoa",
            Quantity = 1,
            UnitPrice = examFee,
            TotalAmount = examFee,
            InsuranceCoveredAmount = examIns,
            PatientPayAmount = examPat
        });

        // Dịch vụ kỹ thuật
        foreach (var order in encounter.ServiceOrders)
        {
            foreach (var detail in order.Details)
            {
                total += (detail.UnitPrice * detail.Quantity);
                totalIns += detail.InsuranceCoveredAmount;
                totalPat += detail.PatientPayAmount;

                billing.Items.Add(new OutpatientBillingItem
                {
                    ItemType = "SERVICE",
                    ItemReferenceId = detail.Id,
                    ItemCode = detail.ServiceCode,
                    ItemName = detail.ServiceName,
                    Quantity = detail.Quantity,
                    UnitPrice = detail.UnitPrice,
                    TotalAmount = detail.UnitPrice * detail.Quantity,
                    InsuranceCoveredAmount = detail.InsuranceCoveredAmount,
                    PatientPayAmount = detail.PatientPayAmount
                });
            }
        }

        // Thuốc
        foreach (var pres in encounter.Prescriptions)
        {
            foreach (var item in pres.Items)
            {
                total += (item.UnitPrice * item.Quantity);
                totalIns += item.InsuranceCoveredAmount;
                totalPat += item.PatientPayAmount;

                billing.Items.Add(new OutpatientBillingItem
                {
                    ItemType = "MEDICINE",
                    ItemReferenceId = item.Id,
                    ItemCode = item.MedicineCode,
                    ItemName = item.MedicineName,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalAmount = item.UnitPrice * item.Quantity,
                    InsuranceCoveredAmount = item.InsuranceCoveredAmount,
                    PatientPayAmount = item.PatientPayAmount
                });
            }
        }

        billing.TotalAmount = total;
        billing.InsuranceCoveredAmount = totalIns;
        billing.PatientPayAmount = totalPat;
        billing.AmountPaid = totalPat;

        dbContext.Billings.Add(billing);
        await dbContext.SaveChangesAsync(cancellationToken);

        var dto = new BillingDto(
            billing.Id,
            billing.EncounterId,
            billing.PatientId,
            billing.InvoiceNumber,
            billing.InvoiceDate,
            billing.TotalAmount,
            billing.InsuranceCoveredAmount,
            billing.PatientPayAmount,
            billing.DiscountAmount,
            billing.AmountPaid,
            billing.PaymentMethod,
            billing.CashierName,
            billing.Status,
            billing.Items.Select(i => new BillingItemDto(
                i.Id, i.ItemType, i.ItemReferenceId, i.ItemCode, i.ItemName,
                i.Quantity, i.UnitPrice, i.TotalAmount, i.InsuranceCoveredAmount, i.PatientPayAmount
            )).ToList()
        );

        return Result.Success(dto);
    }
}

