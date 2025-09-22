namespace AdPlatforms.Back.Common.Results;

public static class ResultFunctionality
{
    public static T Match<T>(this Result res, Func<T> onSuccess, Func<Exception, T> onFail)
        => res.Success ? onSuccess() : onFail(res.Exception);

    public static Result ToResult(this bool value) => value;
}