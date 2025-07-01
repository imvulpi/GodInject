using System.Collections;

namespace GodInject.Prebuild.API.collections
{
    /// <summary>
    /// List supporting manipulation of ranges
    /// A wrapper around regular system List, since it supports ranges.
    /// </summary>
    public class RangeList<T> : IRangeList<T>
    {
        private readonly List<T> _list;

        public RangeList()
        {
            _list = new List<T>();
        }

        public RangeList(IEnumerable<T> items)
        {
            _list = new List<T>(items);
        }

        public RangeList(int initialCapacity)
        {
            _list = new List<T>(initialCapacity);
        }

        public void Add(T item) => _list.Add(item);
        public void Clear() => _list.Clear();
        public bool Contains(T item) => _list.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
        public bool Remove(T item) => _list.Remove(item);
        public int Count => _list.Count;
        public bool IsReadOnly => false;

        public T this[int index] { get => _list[index]; set => _list[index] = value; }

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

        // Range operations
        public void AddRange(IEnumerable<T> items) => _list.AddRange(items);

        public void RemoveRange(IEnumerable<T> items)
        {
            foreach (var item in items)
                _list.Remove(item);
        }

        public void InsertRange(int index, IEnumerable<T> items) => _list.InsertRange(index, items);

        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
        }
    }
}
