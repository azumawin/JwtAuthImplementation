namespace JwtAuthImplementation.Shared;

public class Result<TValue, TError>
{
    public TValue? Value { get; }
    public TError? Error { get; }
    public bool IsSuccess { get; }

    public Result(TValue value)
    {
        Value = value;
        IsSuccess = true;
    }

    public Result(TError error)
    {
        Error = error;
        IsSuccess = false;
    }

    public static implicit operator Result<TValue, TError>(TValue value) => new(value);

    public static implicit operator Result<TValue, TError>(TError error) => new(error);
}
