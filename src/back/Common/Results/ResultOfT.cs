using AdPlatforms.Back.Common.Results.Exceptions;

namespace AdPlatforms.Back.Common.Results;

public readonly record struct Result<T>
{
    readonly T? _value;
    readonly Func<Exception>? _exceptor;

    public T Value => Success ? _value! : throw Exception;
    public Exception Exception => Failure ? _exceptor!() : throw new InvalidOperationException();
    public bool Success => _exceptor is null;
    public bool Failure => _exceptor is { };

    public Result() => _exceptor = () => new UninitailizedException();
    Result(T? value, Func<Exception>? exception = null) => (_value, _exceptor)
        = (value, exception) is (null, null) ? (default, () => new NullValueException())
        : (value, exception) is (_, { }) ? (default, exception)
        : (value, default(Func<Exception>));

    public static implicit operator Result<T>(T? value) => new(value);
    public static implicit operator Result<T>(Exception exception) => new(default, exception is { } ? () => exception : null);
    public static implicit operator Result<T>(Func<Exception> exceptor) => new(default, exceptor);
}