namespace AdPlatforms.Common.Results;

public static class ResultAsyncFunctionality
{
    public static ValueTask<T> ToValue<T>(this Task<T> task) => new(task);


    public static async ValueTask<T> MatchAsync<T>(this ValueTask<Result> task, Func<T> onSuccess, Func<Exception, T> onFail) => (await task).Match(onSuccess, onFail);
    public static async ValueTask<T> MatchAsync<T>(this ValueTask<Result> task, Func<Task<T>> onSuccess, Func<Exception, Task<T>> onFail) => await (await task).Match(onSuccess, onFail);
}