namespace AdPlatforms.Back.Common.Results;

public static class ResultAsyncFunctionality
{
    public static ValueTask<T> ToValue<T>(this Task<T> task) => new(task);


    public static async ValueTask<R> MatchAsync<T, R>(this ValueTask<Result<T>> task, Func<T, R> onSuccess, Func<Exception, R> onError) => (await task).Match(onSuccess, onError);
    public static async ValueTask<R> MatchAsync<T, R>(this ValueTask<Result<T>> task, Func<T, Task<R>> onSuccess, Func<Exception, Task<R>> onError) => await (await task).Match(onSuccess, onError);

    public static async ValueTask<T> OrDefaultAsync<T>(this ValueTask<Result<T>> task, T onError) => (await task).OrDefault(onError);
    public static async ValueTask<T> OrDefaultAsync<T>(this ValueTask<Result<T>> task, Func<T> onError) => (await task).OrDefault(onError);
    public static async ValueTask<T> OrDefaultAsync<T>(this ValueTask<Result<T>> task, Func<Task<T>> onError) => (await task).OrDefault(await onError());
    public static async ValueTask<T?> ToNullableAsync<T>(this ValueTask<Result<T>> task) => (await task).ToNullable();
}