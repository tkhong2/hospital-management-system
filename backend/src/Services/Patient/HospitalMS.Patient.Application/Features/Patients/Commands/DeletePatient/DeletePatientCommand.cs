using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Patient.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Patient.Application.Features.Patients.Commands.DeletePatient;

public record DeletePatientCommand(Guid Id) : IRequest<Result>;

public class DeletePatientCommandHandler(IPatientDbContext dbContext)
    : IRequestHandler<DeletePatientCommand, Result>
{
    public async Task<Result> Handle(DeletePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await dbContext.Patients
            .FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, cancellationToken);

        if (patient is null)
        {
            return Result.Failure("Không tìm thấy hồ sơ bệnh nhân.");
        }

        patient.IsDeleted = true;
        patient.DeletedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}

