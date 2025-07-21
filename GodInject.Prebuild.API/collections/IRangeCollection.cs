namespace GodInject.Prebuild.API.collections
{
    /// <summary>
    /// Represents a collection that supports operations on ranges of elements,
    /// such as adding, removing or inserting a subset of collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    public interface IRangeCollection<T> : ICollection<T>
    {
        /// <summary>
        /// Adds a collection of items to the end of the current collection.
        /// </summary>
        /// <param name="items">The items to add to the collection.</param>
        void AddRange(IEnumerable<T> items);

        /// <summary>
        /// Removes a collection of items from the current collection, if they exist.
        /// </summary>
        /// <param name="items">The items to remove from the collection.</param>
        void RemoveRange(IEnumerable<T> items);

        /// <summary>
        /// Inserts a collection of items starting at the specified index.
        /// </summary>
        /// <param name="index">The index at which to begin inserting the items.</param>
        /// <param name="items">The items to insert into the collection.</param>
        void InsertRange(int index, IEnumerable<T> items);
    }
}
