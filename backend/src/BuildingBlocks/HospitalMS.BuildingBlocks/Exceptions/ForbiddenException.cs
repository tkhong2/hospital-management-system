namespace HospitalMS.BuildingBlocks.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException(string? message = null)
        : base(message ?? "You do not have permission to perform this action.", 403)
    {
    }
}
