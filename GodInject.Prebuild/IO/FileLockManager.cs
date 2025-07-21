using System.Collections.Concurrent;

/// <summary>
/// Manages file locks for specific paths, makes async operations on files more secure.
/// </summary>
public static class FileLockManager
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    /// <summary>
    /// Gets or adds a new <see cref="SemaphoreSlim"/> lock that you can await
    /// </summary>
    /// <remarks>
    /// No need to release the lock just use the await <c>using</c> var _ = await FileLockManager.WaitAsync(path);
    /// </remarks>
    /// <param name="path"></param>
    /// <returns></returns>
    public static async Task<IAsyncDisposable> WaitAsync(string path)
    {
        var normalizedPath = Path.GetFullPath(path).ToLowerInvariant();

        var sem = _locks.GetOrAdd(normalizedPath, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync();

        return new AsyncReleaser(sem);
    }

    /// <summary>
    /// Releaser file for automatic release of locks
    /// </summary>
    /// <param name="semaphore">Semaphore lock to release</param>
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
