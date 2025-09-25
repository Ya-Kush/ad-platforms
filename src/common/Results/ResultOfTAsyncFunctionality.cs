namespace AdPlatforms.Common.Results;

public static class ResultOfTAsyncFunctionality
{
    public static async ValueTask<R> MatchAsync<T, R>(this Result<T> res, Func<T, Task<R>> onSuccess, Func<Exception, R> onFail) => res.Success ? await onSuccess(res.Value).AsValue() : onFail(res.Exception);
    public static async ValueTask<R> MatchAsync<T, R>(this ValueTask<Result<T>> task, Func<T, R> onSuccess, Func<Exception, R> onFail) => (await task).Match(onSuccess, onFail);
    public static async ValueTask<R> MatchAsync<T, R>(this ValueTask<Result<T>> task, Func<T, Task<R>> onSuccess, Func<Exception, Task<R>> onFail) => await (await task).Match(onSuccess, onFail);

    public static async ValueTask<T> OrDefaultAsync<T>(this ValueTask<Result<T>> task, T onFail) => (await task).OrDefault(onFail);
    public static async ValueTask<T> OrDefaultAsync<T>(this ValueTask<Result<T>> task, Func<T> onFail) => (await task).OrDefault(onFail);
    public static async ValueTask<T> OrDefaultAsync<T>(this ValueTask<Result<T>> task, Func<Task<T>> onFail) => (await task).OrDefault(await onFail());
    public static async ValueTask<T?> ToNullableAsync<T>(this ValueTask<Result<T>> task) => (await task).ToNullable();
}