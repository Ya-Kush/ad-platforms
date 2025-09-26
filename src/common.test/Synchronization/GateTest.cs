namespace AdPlatforms.Common.Synchronization.Test;

public sealed class GateTest
{
    [Fact]
    public async Task Cross_Opened()
    {
        var gate = new Gate();
        var keys = new string[] { "a", "b" };
        await Assert.AllAsync(keys, k => gate.CrossAsync(k).AsTask());
    }

    [Fact]
    public async Task Cross_Closing()
    {
        var gate = new Gate();
        var keys = new string[] { "a", "b" };

        await gate.CloseAsync(this);
        await Task.Delay(10);
        var tasks = keys.Select(k => gate.CrossAsync(k).AsTask()).ToArray();
        Assert.Equal([false, false], tasks.Select(t => t.IsCompleted));

        await gate.OpenAsync(this);
        await Task.Delay(10);
        Assert.Equal([true, true], tasks.Select(t => t.IsCompleted));
    }

    [Fact]
    public async Task Cross_CloserOnly()
    {
        var gate = new Gate();
        var keys = new string[] { "a", "b" };

        await gate.CloseAsync(keys[0]);
        await Task.Delay(10);
        var tasks = keys.Select(k => gate.CrossAsync(k).AsTask()).ToArray();
        Assert.Equal([true, false], tasks.Select(t => t.IsCompleted));
    }

    [Fact]
    public async Task ScopedClose()
    {
        var gate = new Gate();
        var keys = new string[] { "a", "b" };
        Task[] tasks;
        await using (var s = await gate.ScopedCloseAsync())
        {
            await Task.Delay(10);
            tasks = keys.Select(k => gate.CrossAsync(k).AsTask()).ToArray();
            Assert.Equal([false, false], tasks.Select(t => t.IsCompleted));

            var t = gate.CrossAsync(s);
            await Task.Delay(10);
            Assert.True(t.IsCompleted);
        }
        Assert.Equal([true, true], tasks.Select(t => t.IsCompleted));
    }
    
    [Fact]
    public async Task ScopedClose_Plural()
    {
        var gate = new Gate();

        await gate.CloseAsync(this);
        var tasks = Enumerable.Range(0, 2)
            .Select(async _ => { await using var s = await gate.ScopedCloseAsync(); })
            .ToArray();

        Assert.Equal([false, false], tasks.Select(t => t.IsCompleted));
        await gate.OpenAsync(this);
        await Task.Delay(20);
        Assert.Equal([true, true], tasks.Select(t => t.IsCompleted));
    }
}
