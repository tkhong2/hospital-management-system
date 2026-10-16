using System.Diagnostics;

namespace HospitalMS.BuildingBlocks.Observability;

public static class HospitalActivitySource
{
    public const string ActivitySourceName = "HospitalMS.Core";
    public static readonly ActivitySource Instance = new(ActivitySourceName, "1.0.0");

    public static Activity? StartHospitalActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        return Instance.StartActivity(name, kind);
    }
}

