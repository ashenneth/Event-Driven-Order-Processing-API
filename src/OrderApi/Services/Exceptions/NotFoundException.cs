namespace OrderApi.Services.Exceptions;

public sealed class NotFoundException : ApiException
{
    public NotFoundException(string message) : base(message) { }
    public override int StatusCode => StatusCodes.Status404NotFound;
}
