using System;
using System.Collections.Generic;

namespace Barbar.WordToVector.Extensions
{
    public static class MergeSortExtension
    {
        private static void Merging<T>(T[] a, T[] b, int low, int mid, int high, IComparer<T> comparer)
        {
            int l1, l2, i;

            for (l1 = low, l2 = mid + 1, i = low; l1 <= mid && l2 <= high; i++)
            {
                if (comparer.Compare(a[l1], a[l2]) <= 0)
                {
                    b[i] = a[l1++];
                }
                else
                {
                    b[i] = a[l2++];
                }
            }

            while (l1 <= mid)
            {
                b[i++] = a[l1++];
            }

            while (l2 <= high)
            {
                b[i++] = a[l2++];
            }

            for (i = low; i <= high; i++)
            {
                a[i] = b[i];
            }
        }

        private static void Sort<T>(T[] a, T[] b, int low, int high, IComparer<T> comparer)
        {
            if (low < high)
            {
                var mid = (low + high) / 2;
                Sort(a, b, low, mid, comparer);
                Sort(a, b, mid + 1, high, comparer);
                Merging(a, b, low, mid, high, comparer);
            }
        }

        public static void MergeSort<T>(this T[] array, int startIndex, int endIndex, IComparer<T> comparer)
        {
            if (array == null || array.Length == 0)
            {
                throw new ArgumentException("Nothing to sort.", nameof(array));
            }
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
            if (endIndex >= array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(endIndex));
            }
            if (startIndex >= endIndex)
            {
                throw new ArgumentException("Invalid index.", nameof(endIndex));
            }
            if (comparer == null)
            {
                throw new ArgumentNullException(nameof(comparer));
            }
            var clone = new T[array.Length];
            Array.Copy(array, clone, endIndex + 1);
            Sort(array, clone, startIndex, endIndex, comparer);
        }
    }
}
