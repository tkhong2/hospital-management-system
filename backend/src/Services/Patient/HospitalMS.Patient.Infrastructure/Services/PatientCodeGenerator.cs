using HospitalMS.Patient.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Patient.Infrastructure.Services;

public class PatientCodeGenerator(IPatientDbContext dbContext) : IPatientCodeGenerator
{
    private static readonly SemaphoreSlim Semaphore = new(1, 1);

    public async Task<string> GenerateCodeAsync(CancellationToken cancellationToken = default)
    {
        await Semaphore.WaitAsync(cancellationToken);
        try
        {
            var prefix = $"BN{DateTime.UtcNow:yyMM}";

            // Find highest current sequence number for the current month prefix
            var lastCode = await dbContext.Patients
                .IgnoreQueryFilters()
                .Where(p => p.PatientCode.StartsWith(prefix))
                .OrderByDescending(p => p.PatientCode)
                .Select(p => p.PatientCode)
                .FirstOrDefaultAsync(cancellationToken);

            var nextSeq = 1;
            if (!string.IsNullOrEmpty(lastCode) && lastCode.Length > prefix.Length)
            {
                var suffix = lastCode[prefix.Length..];
                if (int.TryParse(suffix, out var parsed))
                {
                    nextSeq = parsed + 1;
                }
            }

            return $"{prefix}{nextSeq:D5}";
        }
        finally
        {
            Semaphore.Release();
        }
    }
}

