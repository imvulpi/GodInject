using GodInject.Prebuild.API.data;
using MemoryPack;

namespace GodInject.Prebuild.IO
{
    public class MemPackDataCoupler<T>(string filePath, MemoryPackSerializerOptions? memPackOptions = null) : IDataCoupler<T> where T : class, new()
    {
        private readonly string _filePath = filePath;
        private readonly MemoryPackSerializerOptions? _memPackOptions = memPackOptions;

        public Task DeleteAsync()
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);

            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync()
        {
            return Task.FromResult(File.Exists(_filePath));
        }

        public async Task<T?> ReadAsync()
        {
            await using var _ = await FileLockManager.WaitAsync(_filePath);
            if (File.Exists(_filePath))
            {
                using FileStream fileStream = new(_filePath, FileMode.Open, FileAccess.ReadWrite);
                return await MemoryPackSerializer.DeserializeAsync<T?>(fileStream, _memPackOptions);
            }
            else
            {
                return null;
            }
        }

        public async Task SaveAsync(T data)
        {
            await using var _ = await FileLockManager.WaitAsync(_filePath);
            byte[] serializedData = MemoryPackSerializer.Serialize<T>(data, _memPackOptions);
            await File.WriteAllBytesAsync(_filePath, serializedData);
        }
    }
}
