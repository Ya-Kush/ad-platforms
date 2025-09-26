using System.Runtime.CompilerServices;

namespace AdPlatforms.Common.Synchronization;

internal sealed class Gate
{
    object? _closedBy;
    public async ValueTask OpenAsync(object who)
    {
        await CrossAsync(who);
        _closedBy = null;
    }

    public async ValueTask CloseAsync(object who)
    {
        await CrossAsync(who);
        _closedBy = who;
    }

    public async ValueTask<Scope> ScopedCloseAsync()
    {
        var scope = new Scope(this);
        await CloseAsync(scope);
        return scope;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async ValueTask CrossAsync(object who)
    {
        while (_closedBy is { } && ReferenceEquals(_closedBy, who) is false)
            await Task.Yield();
    }

    public sealed class Scope(Gate gate) : IDisposable, IAsyncDisposable
    {
        public void Dispose() => DisposeAsync().GetAwaiter().GetResult();
        public async ValueTask DisposeAsync() => await gate.OpenAsync(this);
    }
}