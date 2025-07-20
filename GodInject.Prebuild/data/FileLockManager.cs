using System.Collections.Concurrent;

public static class FileLockManager
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public static async Task<IAsyncDisposable> WaitAsync(string path)
    {
        var normalizedPath = Path.GetFullPath(path).ToLowerInvariant();

        var sem = _locks.GetOrAdd(normalizedPath, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync();

        return new AsyncReleaser(sem);
    }

    private sealed class AsyncReleaser(SemaphoreSlim semaphore) : IAsyncDisposable
    {
        private readonly SemaphoreSlim _semaphore = semaphore;
        private bool _disposed;

        public ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                _semaphore.Release();
                _disposed = true;
            }

            return ValueTask.CompletedTask;
        }
    }
}
