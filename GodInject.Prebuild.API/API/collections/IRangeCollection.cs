namespace GodInject.Prebuild.utils.collections
{
    public interface IRangeCollection<T> : ICollection<T>
    {
        void AddRange(IEnumerable<T> items);
        void RemoveRange(IEnumerable<T> items);
        void InsertRange(int index, IEnumerable<T> items);
    }
}
