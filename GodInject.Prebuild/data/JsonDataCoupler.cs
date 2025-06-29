using GodInject.Prebuild.API.data;
using System.Text.Json;

namespace GodInject.Prebuild.data
{
    public class JsonDataCoupler<T> : IDataCoupler<T> where T : class, new()
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions = new(){};

        public JsonDataCoupler(string filePath, JsonSerializerOptions? serializerOptions = null)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            if(serializerOptions != null)
            {
                _jsonOptions = serializerOptions;
            }
        }

        public Task<bool> ExistsAsync()
        {
            return Task.FromResult(File.Exists(_filePath));
        }

        public async Task<T?> ReadAsync()
        {
            if (!File.Exists(_filePath))
                return null;

            using var stream = File.OpenRead(_filePath);
            var data = await JsonSerializer.DeserializeAsync<T>(stream, _jsonOptions);
            return data;
        }

        public async Task SaveAsync(T data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            using var stream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(stream, data, _jsonOptions);
        }

        public Task DeleteAsync()
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);

            return Task.CompletedTask;
        }
    }
}
