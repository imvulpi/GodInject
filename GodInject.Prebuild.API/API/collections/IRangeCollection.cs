namespace GodInject.Prebuild.API.collections
{
    /// <summary>
    /// Interface for collections that supports manipulating ranges of a collection
    /// </summary>
    public interface IRangeCollection<T> : ICollection<T>
    {
        void AddRange(IEnumerable<T> items);
        void RemoveRange(IEnumerable<T> items);
        void InsertRange(int index, IEnumerable<T> items);
    }
}
