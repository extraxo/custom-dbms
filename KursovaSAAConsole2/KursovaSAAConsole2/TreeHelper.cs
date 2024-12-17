using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    static class TreeHelper
    {
        public static void RemoveRange<T>(CustomList<T> target, int fromIndex)
        {
            target.RemoveRange(fromIndex, target.Count - fromIndex);
        }

        public static int BinarySearchFirst<T>(CustomList<T> array, T value, IComparer<T> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            var result = array.BinarySearch(value, comparer);
            if (result >= 1)
            {
                var lastI = result;
                for (var i = (result - 1); i >= 0; i--)
                {
                    if (comparer.Compare(array[i], value) != 0)
                    {
                        break;
                    }
                    else
                    {
                        lastI = i;
                    }
                }
                result = lastI;
            }
            return result;
        }

        public static int BinarySearchLast<T>(CustomList<T> array, T value, IComparer<T> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException("comparer");

            var result = array.BinarySearch(value, comparer);

            if ((result >= 0) && ((result + 1) < array.Count))
            {
                var lastI = result;
                for (var i = result + 1; i < array.Count; i++)
                {
                    if (comparer.Compare(array[i], value) != 0)
                    {
                        break;
                    }
                    else
                    {
                        lastI = i;
                    }
                }
                result = lastI;
            }
            return result;
        }
    }
}

