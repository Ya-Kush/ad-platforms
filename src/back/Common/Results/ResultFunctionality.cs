namespace AdPlatforms.Back.Common.Results;

public static class ResultFunctionality
{
    public static R Match<T, R>(this Result<T> res, Func<T, R> onSuccess, Func<Exception, R> onError)
        => res.Successful ? onSuccess(res.Value) : onError(res.Exception);

    public static T OrDefault<T>(this Result<T> res, T @default) => res.Successful ? res.Value : @default;
    public static T OrDefault<T>(this Result<T> res, Func<T> onError) => res.Successful ? res.Value : onError();
    public static T? ToNullable<T>(this Result<T> res) => res.Successful ? res.Value : default;
}