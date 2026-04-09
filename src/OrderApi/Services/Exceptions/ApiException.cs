namespace OrderApi.Services.Exceptions;

public abstract class ApiException : Exception
{
    protected ApiException(string message) : base(message) { }

    public abstract int StatusCode { get; }
    public virtual string ErrorCode => GetType().Name;
    public virtual IDictionary<string, object?>? Details => null;
}
