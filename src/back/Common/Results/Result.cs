namespace AdPlatforms.Back.Common.Results;

public static class Result
{
    public static Result<T> ToResult<T>(this T? value) => value;
    public static Result<T> New<T>(T? value) => value;
    public static Result<T> New<T>(T? value, Exception onNull) => value is { } ? value : onNull;
    public static Result<T> New<T>(T? value, Func<Exception> onNull) => value is { } ? value : onNull;

    public static Result<T> Error<T>(Exception exception) => exception;
    public static Result<T> Error<T>(Func<Exception> exceptor) => exceptor;

    public static Result<T> Try<T>(Func<T?> getter) { try { return getter(); } catch (Exception e) { return e; }}
    public static Result<T> Try<T>(Func<T?> getter, Exception onError) { try { return getter(); } catch { return onError; }}
    public static Result<T> Try<T>(Func<T?> getter, Func<Exception> onError)
    {
        try { return getter(); }
        catch {
            try { return onError(); }
            catch (Exception e) { return e; }
        }
    }
}