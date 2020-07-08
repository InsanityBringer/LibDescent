﻿using System.Collections.Generic;
using System.Linq;

namespace LibDescent.Data
{
    /// <summary>
    /// A sparse array; that is, an array designed to contain a wide range of indexes,
    /// but only be filled with some of those indexes, yet still remain memory-efficient.
    /// </summary>
    /// <typeparam name="T">The type this array will contain.</typeparam>
    public class SparseArray<T>
    {
        private Dictionary<int, T> store;

        public SparseArray()
        {
            store = new Dictionary<int, T>();
        }

        /// <summary>
        /// Gets or sets an item within the given index in this sparse array.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns>The element at the given index, or the default value for that type if not present.</returns>
        public T this[int index]
        {
            get
            {
                if (store.ContainsKey(index))
                    return store[index];
                else
                    return default;
            }
            set
            {
                store[index] = value;
            }
        }

        /// <summary>
        /// Determines whether the given index is in this array.
        /// </summary>
        /// <param name="index">The zero-based index.</param>
        /// <returns>Whether the givenm index has an element in this array.</returns>
        public bool HasIndex(int index) => store.ContainsKey(index);

        /// <summary>
        /// Removes all elements from this sparse array.
        /// </summary>
        public void Clear() => store.Clear();
        
        /// <summary>
        /// The lowest zero-based index such that there is no element with the given
        /// index nor any higher index.
        /// </summary>
        public int Capacity => 1 + store.Keys.DefaultIfEmpty(-1).Max();

        /// <summary>
        /// The total number of elements in this sparse array.
        /// </summary>
        public int Count => store.Keys.Count;
    }
}