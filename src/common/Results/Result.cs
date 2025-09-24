using AdPlatforms.Common.Results.Exceptions;

namespace AdPlatforms.Common.Results;

public readonly partial record struct Result
{
    readonly Func<Exception>? _exceptor;

    public Exception Exception => Failure ? _exceptor!() : throw new InvalidOperationException();
    public bool Success => _exceptor is null;
    public bool Failure => _exceptor is { };

    public Result() => _exceptor = () => new UninitailizedException();
    Result(Func<Exception>? exceptor) => _exceptor = exceptor;

    public static implicit operator Result(bool success) => new(success ? null : () => new FailureException());
    public static implicit operator Result(Exception exception) => new(() => exception ?? new FailureException());
    public static implicit operator Result(Func<Exception> exceptor) => new(exceptor);
}