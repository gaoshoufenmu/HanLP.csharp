﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HanLP.csharp.utility
{
    public class ArrayHelper
    {
        /// <summary>
        /// 二分法查找
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static int BinarySearch<T>(T[] array, T d) where T : IComparable<T>
        {
            if (array.Length <= 0) return -1;   // 没有找到目标对象，如果要将目标按顺序插入到数组容器中，则插入位置应该为 0

            var high = array.Length - 1;
            var low = 0;

            while(low <= high)
            {
                var mid = (low + high) >> 1;
                int cmp = array[mid].CompareTo(d);
                if (cmp < 0)
                    low = mid + 1;
                else if (cmp > 0)
                    high = mid - 1;
                else
                    return mid;
            }
            return -low - 1;    // 没有找到目标对象，如果要将目标按顺序插入到数组容器中，则插入位置应该为 low，为了与上面返回-1 一致，作 -low-1 处理
        }

        public static int BinarySearch<T>(T[] a, int from, int len, T key) where T : IComparable<T>
        {
            if (a.Length <= 0) return -1;
            var low = from;
            var high = from + len - 1;
            if (a.Length < from + len)
                high = a.Length - 1;

            while(low <= high)
            {
                var mid = (low + high) >> 1;
                int cmp = a[mid].CompareTo(key);
                if (cmp < 0)
                    low = mid + 1;
                else if (cmp > 0)
                    high = mid - 1;
                else
                    return mid;
            }
            return -low - 1; // 没有找到目标对象，如果要将目标按顺序插入到数组容器中，则插入位置应该为 low，为了与上面返回-1 一致，作 -low-1 处理
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="dest_hc">目标的哈希值</param>
        /// <returns></returns>
        public static int BinarySearchByHashcode<T>(T[] array, int dest_hc)
        {
            if (array.Length <= 0) return -1;   // 没有找到目标对象，如果要将目标按顺序插入到数组容器中，则插入位置应该为 0

            var high = array.Length - 1;
            var low = 0;

            while (low <= high)
            {
                var mid = (low + high) >> 1;
                var arr_hc = array[mid].GetHashCode();
                
                if (arr_hc < dest_hc)
                    low = mid + 1;
                else if (arr_hc > dest_hc)
                    high = mid - 1;
                else
                    return mid;
            }
            return -low - 1;    // 没有找到目标对象，如果要将目标按顺序插入到数组容器中，则插入位置应该为 low，为了与上面返回-1 一致，作 -low-1 处理
        }

        public static void Sort<T>(T[] array, Func<T, T, bool> comparator) => Sort(array, 0, array.Length - 1, comparator);

        public static void Sort<T>(T[] array, int left, int right, Func<T, T, bool> comparator)
        {
            if(left < right)
            {
                var middle = Partition<T>(array, left, right, comparator);
                Sort(array, left, middle - 1, comparator);
                Sort(array, middle + 1, right, comparator);
            }
        }
        private static int Partition<T>(T[] array, int start, int end, Func<T, T, bool> comparator)
        {
            var pivot = array[start];
            while(start < end)
            {
                while (start < end && comparator(pivot, array[end])) end--;
                if (start < end)
                    array[start++] = array[end];
                while (start < end && comparator(array[start], pivot)) start++;
                if (start < end)
                    array[end--] = array[start];
            }
            array[start] = pivot;
            return start;
        }
    }
}
