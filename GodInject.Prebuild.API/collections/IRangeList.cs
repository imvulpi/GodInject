namespace GodInject.Prebuild.API.collections
{
    /// <summary>
    /// Represents a list that supports ranged   operations on contiguous ranges of elements,
    /// such as inserting, removing, or adding multiple items at specific positions.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    public interface IRangeList<T> : IRangeCollection<T>, IList<T>
    {
    }
}
