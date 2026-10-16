using System.Diagnostics.Metrics;

namespace HospitalMS.BuildingBlocks.Observability;

public static class HospitalMetrics
{
    public const string MeterName = "HospitalMS.System";
    private static readonly Meter Meter = new(MeterName, "1.0.0");

    // Counters
    public static readonly Counter<long> OutpatientEncounters =
        Meter.CreateCounter<long>("hospitalms_encounters_total", description: "Tổng số lượt khám bệnh ngoại trú");

    public static readonly Counter<long> PrescriptionsCreated =
        Meter.CreateCounter<long>("hospitalms_prescriptions_total", description: "Tổng số đơn thuốc được kê");

    public static readonly Counter<long> PrescriptionsDispensed =
        Meter.CreateCounter<long>("hospitalms_prescriptions_dispensed_total", description: "Tổng số đơn thuốc đã cấp phát tại kho");

    public static readonly Counter<long> HealthExamRegistrations =
        Meter.CreateCounter<long>("hospitalms_exam_registrations_total", description: "Tổng số lượt đăng ký khám sức khỏe");

    public static readonly Counter<long> InpatientAdmissions =
        Meter.CreateCounter<long>("hospitalms_inpatient_admissions_total", description: "Tổng số lượt nhập viện nội trú");

    public static readonly Counter<long> OutboxEventsPublished =
        Meter.CreateCounter<long>("hospitalms_outbox_events_published_total", description: "Tổng số integration events publish qua Outbox");

    // Gauges / UpDownCounters
    public static readonly UpDownCounter<int> ActiveBedsOccupied =
        Meter.CreateUpDownCounter<int>("hospitalms_beds_occupied", description: "Số giường bệnh đang có bệnh nhân nằm");
}

