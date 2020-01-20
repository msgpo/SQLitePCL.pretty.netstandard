using System.Collections.Generic;

namespace SQLitePCL.pretty
{
    internal sealed class OrderedSet<T> : ICollection<T>
    {
        private readonly List<T> _list = new List<T>();

        public void Add(T item)
        {
            if (!_list.Contains(item))
            {
                _list.Add(item);
            }
        }

        public void Clear()
            => _list.Clear();

        public bool Contains(T item)
            => _list.Contains(item);

        public void CopyTo(T[] array, int arrayIndex)
            => _list.CopyTo(array, arrayIndex);

        public int Count => _list.Count;

        public bool IsReadOnly => false;

        public bool Remove(T item)
            => _list.Remove(item);

        public IEnumerable<T> Reverse()
        {
            var len = _list.Count;
            for (int i = len - 1; i >= 0; i--)
            {
                yield return _list[i];
            }
        }

        public IEnumerator<T> GetEnumerator()
            => this._list.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => this.GetEnumerator();
    }
}
