namespace HospitalMS.BuildingBlocks.Exceptions;

public class NotFoundException : AppException
{
    public NotFoundException(string entityName, object key)
        : base($"Entity \"{entityName}\" ({key}) was not found.", 404)
    {
    }
}
