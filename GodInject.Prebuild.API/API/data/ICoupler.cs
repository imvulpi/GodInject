namespace GodInject.Prebuild.API.data
{
    /// <summary>
    /// Interface for CRUD-like operations on stored data.
    /// </summary>
    public interface IDataCoupler<T> where T : class, new()
    {
        Task<bool> ExistsAsync();
        Task<T?> ReadAsync();
        Task SaveAsync(T data);
        Task DeleteAsync();
    }
}
