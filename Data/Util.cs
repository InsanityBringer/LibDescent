﻿/*
    Copyright (c) 2019 SaladBadger

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

using LibDescent.Data.Midi;
using System;
using System.Collections.Generic;

namespace LibDescent.Data
{
    public class Util
    {
        /// <summary>
        /// Clamps a given value between the minimum and maximum; that is, the returned
        /// value is always between the minimum and the maximum, and will be either of
        /// the extrema if the value is beyond either of them.
        /// </summary>
        /// <param name="x">The value to clamp.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <param name="changed">Whether the value was outside the range.</param>
        /// <returns>The value clamped between the minimum and maximum.</returns>
        public static int Clamp(int x, int min, int max, out bool changed)
        {
            if (x < min)
            {
                changed = true;
                return min;
            }
            if (x > max)
            {
                changed = true;
                return max;
            }
            changed = false;
            return x;
        }

        /// <summary>
        /// Clamps a given value between the minimum and maximum; that is, the returned
        /// value is always between the minimum and the maximum, and will be either of
        /// the extrema if the value is beyond either of them.
        /// </summary>
        /// <param name="x">The value to clamp.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns>The value clamped between the minimum and maximum.</returns>
        public static int Clamp(int x, int min, int max)
        {
            return Clamp(x, min, max, out bool _);
        }

        /// <summary>
        /// Swaps two items in an array.
        /// </summary>
        /// <typeparam name="T">The type of the array.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="a">The first zero-based index.</param>
        /// <param name="b">The second zero-based index.</param>
        public static void Swap<T>(T[] array, int a, int b)
        {
            T tmp = array[a];
            array[a] = array[b];
            array[b] = tmp;
        }

        /// <summary>
        /// Sorts a list using a stable sort; that is, if the list contains two items
        /// A and B, which are considered equal with the default comparer, such that A
        /// is always before element B in the list, then A will always remain before
        /// element B in the sorted list; the mutual order of equivalent-by-comparison
        /// items is not affected.
        /// </summary>
        /// <typeparam name="T">The type of the list.</typeparam>
        /// <param name="list">The list to sort.</param>
        public static void StableSort<T>(List<T> list)
        {
            StableSort(list, Comparer<T>.Default);
        }

        /// <summary>
        /// Sorts a list using a stable sort; that is, if the list contains two items
        /// A and B, which are considered equal with the given comparer, such that A
        /// is always before element B in the list, then A will always remain before
        /// element B in the sorted list; the mutual order of equivalent-by-comparison
        /// items is not affected.
        /// </summary>
        /// <typeparam name="T">The type of the list.</typeparam>
        /// <param name="list">The list to sort.</param>
        /// <param name="comparer">The comparer to use for the sorting.</param>
        public static void StableSort<T>(List<T> list, IComparer<T> comparer)
        {
            // merge sort
            T[] array = list.ToArray();
            T[] buf = new T[list.Count];
            
            int listA, listB, listAe, listBe, listOffset, listSize = 1, bufptr;
            for (int groupSize = 2; listSize < array.Length; groupSize <<= 1)
            {
                for (listOffset = 0; listOffset < array.Length; listOffset += groupSize)
                {
                    listA = listOffset;
                    listB = listAe = listA + listSize;
                    listBe = Math.Min(listB + listSize, array.Length);

                    if (listBe <= listB)
                        break;

                    bufptr = 0;
                    
                    while (listA < listAe && listB < listBe)
                    {
                        if (comparer.Compare(array[listA], array[listB]) <= 0)
                            buf[bufptr++] = array[listA++];
                        else
                            buf[bufptr++] = array[listB++];
                    }
                    while (listA < listAe)
                        buf[bufptr++] = array[listA++];
                    while (listB < listBe)
                        buf[bufptr++] = array[listB++];

                    Array.Copy(buf, 0, array, listOffset, bufptr);
                }
                listSize = groupSize;
            }

            list.Clear();
            list.AddRange(array);
        }
    }
}
