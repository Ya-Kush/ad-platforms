namespace AdPlatforms.Common.Results;

public static class ResultOfTFunctionality
{
    public static R Match<T, R>(this Result<T> res, Func<T, R> onSuccess, Func<Exception, R> onFail) => res.Success ? onSuccess(res.Value) : onFail(res.Exception);

    public static T OrDefault<T>(this Result<T> res, T @default) => res.Success ? res.Value : @default;
    public static T OrDefault<T>(this Result<T> res, Func<T> onFail) => res.Success ? res.Value : onFail();
    public static T? ToNullable<T>(this Result<T> res) => res.Success ? res.Value : default;

    public static Result<T> ToResult<T>(this T? value) => value;
}