namespace GodInject.Prebuild.API.data
{
    /// <summary>
    /// Interface for CRUD-like asynchronous operations on stored data of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of data to be managed</typeparam>
    public interface IDataCoupler<T> where T : class, new()
    {
        /// <summary>
        /// Checks asynchronously if the data exists.
        /// </summary>
        /// <returns>A task that resolves to <c>true</c> if data exists; otherwise, <c>false</c>.</returns>
        Task<bool> ExistsAsync();

        /// <summary>
        /// Reads the data asynchronously.
        /// </summary>
        /// <returns>A task that resolves to the data if it exists; otherwise, <c>null</c>.</returns>
        Task<T?> ReadAsync();

        /// <summary>
        /// Saves the specified data asynchronously.
        /// </summary>
        /// <param name="data">The data to save.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        Task SaveAsync(T data);

        /// <summary>
        /// Deletes the data asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous delete operation.</returns>
        Task DeleteAsync();
    }
}
