using AdPlatforms.Back.Common.Results.Exceptions;

namespace AdPlatforms.Back.Common.Results;

public readonly partial record struct Result
{
    readonly Func<Exception>? _exceptor;

    public Exception Exception => Failure ? _exceptor!() : throw new InvalidOperationException();
    public bool Success => _exceptor is null;
    public bool Failure => _exceptor is { };

    public Result() => _exceptor = () => new UninitailizedException();
    Result(Func<Exception>? exception = null) => _exceptor = exception is null
        ? () => new NullValueException() : default;

    public static implicit operator Result(bool success) => new(success ? default : () => new FailureException());
    public static implicit operator Result(Exception? exception) => new(exception is { } ? () => exception : null);
    public static implicit operator Result(Func<Exception> exceptor) => new(exceptor);
}