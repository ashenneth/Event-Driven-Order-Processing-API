namespace OrderApi.Services.Exceptions;

public sealed class ValidationException : ApiException
{
    private readonly Dictionary<string, string[]> _errors;

    public ValidationException(string message, Dictionary<string, string[]> errors) : base(message)
    {
        _errors = errors;
    }

    public override int StatusCode => StatusCodes.Status400BadRequest;

    public override IDictionary<string, object?>? Details =>
        new Dictionary<string, object?>
        {
            ["errors"] = _errors
        };
}
