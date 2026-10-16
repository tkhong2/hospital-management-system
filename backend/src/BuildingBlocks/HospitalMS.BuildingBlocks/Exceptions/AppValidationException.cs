namespace HospitalMS.BuildingBlocks.Exceptions;

public class AppValidationException : AppException
{
    public IReadOnlyCollection<ValidationError> Errors { get; }

    public AppValidationException(IEnumerable<ValidationError> errors)
        : base("One or more validation errors occurred.", 400)
    {
        Errors = errors.ToList().AsReadOnly();
    }
}

public record ValidationError(string PropertyName, string ErrorMessage);
