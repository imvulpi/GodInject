namespace GodInject.Prebuild.API.data
{
    public interface IDataCoupler<T> where T : class, new()
    {
        Task<bool> ExistsAsync();
        Task<T?> ReadAsync();
        Task SaveAsync(T data);
        Task DeleteAsync();
    }
}
