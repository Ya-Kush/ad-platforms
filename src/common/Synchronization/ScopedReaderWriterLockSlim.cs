namespace AdPlatforms.Common.Synchronization;

public sealed class ScopedReaderWriterLockSlim : IDisposable
{
    readonly ReaderWriterLockSlim _lock = new();

    public ReaderScope ScopedRead() => new(_lock);
    public WriterScope ScopedWrite() => new(_lock);
    public void Dispose() => _lock.Dispose();

    public readonly struct ReaderScope : IDisposable
    {
        readonly ReaderWriterLockSlim _lock;
        public ReaderScope(ReaderWriterLockSlim @lock)
        {
            _lock = @lock;
            _lock.EnterReadLock();
        }
        public void Dispose() => _lock.ExitReadLock();
    }
    public readonly struct WriterScope : IDisposable
    {
        readonly ReaderWriterLockSlim _lock;
        public WriterScope(ReaderWriterLockSlim @lock)
        {
            _lock = @lock;
            _lock.EnterWriteLock();
        }
        public void Dispose() => _lock.ExitWriteLock();
    }
}