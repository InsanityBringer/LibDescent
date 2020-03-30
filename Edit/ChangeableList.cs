using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LibDescent.Edit
{
    /// <summary>
    /// A wrapper for List&lt;<typeparamref name="T"/>&gt; that is itself a ChangeableState
    /// and will pass on events whenever any item (.Items) or the number of items (.Count) changes.
    /// </summary>
    /// <typeparam name="T">The type to store in this list.</typeparam>
    public class ChangeableList<T> : ChangeableState, IList<T>
    {
        private List<T> _list;

        public ChangeableList() : base() => _list = new List<T>();
        public ChangeableList(int capacity) : base() => _list = new List<T>(capacity);
        public ChangeableList(IEnumerable<T> collection) : base() => _list = new List<T>(collection);

        public T this[int index]
        { 
            get => _list[index];
            set
            {
                _list[index] = value;
                OnListUpdated();
            }
        }

        public int Count => _list.Count;

        /// <summary>
        /// The inner list used by this ChangeableList. Listen to this property
        /// to listen for events within items in this list, but do not modify the
        /// list itself by using it.
        /// </summary>
        public List<T> Items => _list;

        public bool IsReadOnly => false;

        private bool OnCountMaybeUpdated(int oldCount)
        {
            return AssignChangedRename(ref oldCount, Count, nameof(Count));
        }

        private void OnListUpdated()
        {
            // cause "List" property change event
            List<T> dummy = null;
            AssignAlwaysRename(ref dummy, _list, nameof(Items));
        }

        public bool Contains(T item) => _list.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
        public int IndexOf(T item) =>  _list.IndexOf(item);
        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

        public void Add(T item)
        {
            int oldCount = Count;
            _list.Add(item);
            if (OnCountMaybeUpdated(oldCount))
                OnListUpdated();
        }

        public void Clear()
        {
            int oldCount = Count;
            _list.Clear();
            if (OnCountMaybeUpdated(oldCount))
                OnListUpdated();
        }

        public void Insert(int index, T item)
        {
            int oldCount = Count;
            _list.Insert(index, item);
            if (OnCountMaybeUpdated(oldCount))
                OnListUpdated();
        }

        public bool Remove(T item)
        {
            int oldCount = Count;
            bool result = _list.Remove(item);
            if (OnCountMaybeUpdated(oldCount))
                OnListUpdated();
            return result;
        }

        public void RemoveAt(int index)
        {
            int oldCount = Count;
            _list.RemoveAt(index);
            if (OnCountMaybeUpdated(oldCount))
                OnListUpdated();
        }

        /// <summary>
        /// Replaces the <see cref="LibDescent.Edit.ChangeableList">ChangeableList</see>&lt;<typeparamref name="T"/>&gt; value at a given index and returns the old value.
        /// </summary>
        /// <param name="index">The zero-based index of the item to exchange.</param>
        /// <param name="newItem">The new item to replace the item at the given index with.</param>
        /// <returns>The old item at the given index.</returns>
        public T Exchange(int index, T newItem)
        {
            T oldItem = this[index];
            this[index] = newItem;
            OnListUpdated();
            return oldItem;
        }
    }
}
