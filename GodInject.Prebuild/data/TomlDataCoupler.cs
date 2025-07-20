using GodInject.Prebuild.API.data;
using System.Text.Json;
using Tomlet;

namespace GodInject.Prebuild.data
{
    public class TomlDataCoupler<T> : IDataCoupler<T> where T : class, new()
    {
        private readonly string _filePath;
        private readonly TomlSerializerOptions _tomlOptions = new(){};

        public TomlDataCoupler(string filePath, TomlSerializerOptions? serializerOptions = null)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            if(serializerOptions != null)
            {
                _tomlOptions = serializerOptions;
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

            await using var _ = await FileLockManager.WaitAsync(_filePath);
            string tomlString = await File.ReadAllTextAsync(_filePath);
            return TomletMain.To<T>(tomlString, _tomlOptions);
        }

        public async Task SaveAsync(T data)
        {
            ArgumentNullException.ThrowIfNull(data);
            await using var _ = await FileLockManager.WaitAsync(_filePath);
            string tomlString = TomletMain.TomlStringFrom(data, _tomlOptions);
            await File.WriteAllTextAsync(_filePath, tomlString);
        }

        public Task DeleteAsync()
        {
            if (File.Exists(_filePath))
                File.Delete(_filePath);

            return Task.CompletedTask;
        }
    }
}
