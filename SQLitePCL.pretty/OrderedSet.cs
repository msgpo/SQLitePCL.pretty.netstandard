using System.Collections.Generic;

namespace SQLitePCL.pretty
{
    internal sealed class OrderedSet<T> : ICollection<T>
    {
        private readonly LinkedList<T> list = new LinkedList<T>();

        public void Add(T item)
        {
            if (list.Contains(item))
            {
                list.AddLast(item);
            }
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(T item) =>
            list.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) =>
            list.CopyTo(array, arrayIndex);

        public int Count => list.Count;

        public bool IsReadOnly => false;

        public bool Remove(T item) => list.Remove(item);

        public IEnumerable<T> Reverse()
        {
            var el = list.Last;
            while (el != null)
            {
                yield return el.Value;
                el = el.Previous;
            }
        }

        public IEnumerator<T> GetEnumerator() =>
            this.list.GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
            this.GetEnumerator();
    }
}
