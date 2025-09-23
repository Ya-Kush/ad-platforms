namespace AdPlatforms.Back.Common.Results;

public readonly partial record struct Result
{
    public static Result Ok() => true;
    public static Result New(bool value) => value;
    public static Result New(bool value, Exception onFail) => value is { } ? value : onFail;
    public static Result New(bool value, Func<Exception> onFail) => value is { } ? value : onFail;

    public static Result Fail() => false;
    public static Result Fail(Exception exception) => exception;
    public static Result Fail(Func<Exception> exceptor) => exceptor;

    public static Result Try(Action trier) { try { trier(); } catch (Exception e) { return e; } return true; }
    public static Result Try(Action trier, Exception onFail) { try { trier(); } catch { return onFail; } return true; }
    public static Result Try(Action trier, Func<Exception> onFail)
    {
        try { trier(); }
        catch
        {
            try { return onFail(); }
            catch (Exception e) { return e; }
        }
        return true;
    }
}