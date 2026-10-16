namespace HospitalMS.BuildingBlocks.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string message) : base(message, 409)
    {
    }
}

public class BusinessRuleException : AppException
{
    public BusinessRuleException(string message) : base(message, 422)
    {
    }
}
