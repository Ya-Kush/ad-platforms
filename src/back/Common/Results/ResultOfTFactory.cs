namespace AdPlatforms.Back.Common.Results;

public readonly partial record struct Result
{
    public static Result<T> New<T>(T? value) => value;
    public static Result<T> New<T>(T? value, Exception onNull) => value is { } ? value : onNull;
    public static Result<T> New<T>(T? value, Func<Exception> onNull) => value is { } ? value : onNull;

    public static Result<T> Fail<T>(Exception exception) => exception;
    public static Result<T> Fail<T>(Func<Exception> exceptor) => exceptor;

    public static Result<T> Try<T>(Func<T?> trier) { try { return trier(); } catch (Exception e) { return e; } }
    public static Result<T> Try<T>(Func<T?> trier, Exception onFail) { try { return trier(); } catch { return onFail; } }
    public static Result<T> Try<T>(Func<T?> trier, Func<Exception> onFail)
    {
        try { return trier(); }
        catch
        {
            try { return onFail(); }
            catch (Exception e) { return e; }
        }
    }
}