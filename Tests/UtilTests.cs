using LibDescent.Data;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LibDescent.Tests
{
    class UtilTests
    {
        [Test]
        public void StableSortTest()
        {
            Random rng = new Random();
            List<PartiallySortable> list = new List<PartiallySortable>();
            int cnt = rng.Next(30, 60);
            int div = rng.Next(3, 9);
            for (int i = 0; i < cnt; ++i)
                list.Add(new PartiallySortable(i / div));
            list.Sort((a, b) => rng.Next(-1, 2)); // shuffle list
            int[] indexes = new int[cnt / div + 1];
            foreach (PartiallySortable ps in list)
                ps.MyIndex = indexes[ps.SortValue]++;

            // finally do stablesort
            Util.StableSort(list, new PartiallySortableComparer());

            // test stable sort
            int lastSortValue = -1, lastMyIndex = -1;
            foreach (PartiallySortable ps in list)
            {
                Assert.GreaterOrEqual(ps.SortValue, lastSortValue);
                if (ps.SortValue > lastSortValue)
                {
                    lastSortValue = ps.SortValue;
                    lastMyIndex = ps.MyIndex;
                }
                else
                    Assert.Greater(ps.MyIndex, lastMyIndex);
            }
        }

        private class PartiallySortableComparer : IComparer<PartiallySortable>
        {
            public int Compare([AllowNull] PartiallySortable x, [AllowNull] PartiallySortable y)
            {
                return x.SortValue - y.SortValue;
            }
        }

        private class PartiallySortable
        {
            internal int SortValue;
            internal int MyIndex;

            internal PartiallySortable(int value)
            {
                SortValue = value;
                MyIndex = 0;
            }
        }
    }
}
