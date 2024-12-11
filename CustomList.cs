using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public class CustomList<T> : ICollection<T>
    {
        private T[] _array;
        private readonly EqualityComparer<T> _equalityComparer;
        public int Count { get; private set; }
        public int Length => Count;
        bool ICollection<T>.IsReadOnly => false;


        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException();
                }
                return _array[index];
            }

            set
            {
                if (index >= Count || index < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _array[index] = value;
            }
        }

        public CustomList()
        {
            _equalityComparer = EqualityComparer<T>.Default;
            _array = new T[4];
        }

        public CustomList(int capacity)
        {
            _equalityComparer = EqualityComparer<T>.Default;
            _array = new T[capacity];
        }

        public CustomList(IEnumerable<T> collection)
        {
            _equalityComparer = EqualityComparer<T>.Default;

            if (collection is ICollection<T> col)
            {
                _array = new T[col.Count];

                col.CopyTo(_array, 0);

                return;
            }

            foreach (var item in collection)
            {
                Add(item);
            }
        }


        private void EnsureCapacity(int requiredCapacity)
        {
            if (_array.Length >= requiredCapacity) return;

            int newCapacity = Math.Max(_array.Length * 2, requiredCapacity);
            var newArray = new T[newCapacity];
            Array.Copy(_array, newArray, Count);
            _array = newArray;
        }

        public void Add(T item)
        {
            EnsureCapacity(Count + 1);
            _array[Count++] = item;
        }

        public void Clear()
        {
            Array.Clear(_array, 0, Count);
            Count = 0;
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < Count; i++)
            {
                if (_equalityComparer.Equals(_array[i], item))
                    return i;
            }

            return -1;
        }

        public void CopyTo(int sourceIndex, T[] destinationArray, int destinationIndex, int count)
        {

            Array.Copy(_array, 0, destinationArray, destinationIndex, Count);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(_array, 0, array, arrayIndex, Count);
        }

        public void Insert(int index, T item)
        {
            if (index > Count)
            {
                throw new IndexOutOfRangeException();
            }

            EnsureCapacity(Count + 1);
            Array.Copy(_array, index, _array, index + 1, Count - index);
            Count++;
        }

        public void RemoveAt(int index)
        {
            Array.Copy(_array, index + 1, _array, index, Count - index - 1);
            Count--;
        }

        public bool Remove(T item)
        {
            var index = IndexOf(item);
            RemoveAt(index);

            if (index < 0)
                return false;

            return true;
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return _array[i];
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int BinarySearch(T item, IComparer<T>? comparer = null)
        {
            comparer ??= Comparer<T>.Default;

            int low = 0;
            int high = Count - 1;

            while (low <= high)
            {
                int mid = low + ((high - low) / 2);
                int comparison = comparer.Compare(_array[mid], item);

                if (comparison == 0)
                {
                    return mid;
                }
                else if (comparison < 0)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            return ~low;
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items is ICollection<T> collection)
            {
                EnsureCapacity(Count + collection.Count);
                collection.CopyTo(_array, Count);
                Count += collection.Count;
            }
            else
            {
                foreach (var item in items)
                {
                    Add(item);
                }
            }
        }
        public void RemoveRange(int index, int count)
        {

            Array.Copy(_array, index + count, _array, index, Count - index - count);

            Count -= count;
            
            Array.Clear(_array, Count, count);

        }

        public void Sort()
        {
            Array.Sort(_array, 0, Count);
        }

        public void Sort(IComparer<T> comparer)
        {
            Array.Sort(_array, 0, Count, comparer);
        }

        public void Sort(Comparison<T> comparison)
        {
            Array.Sort(_array, 0, Count, Comparer<T>.Create(comparison));
        }
        public int BinarySearchFirst(T item, IComparer<T> comparer = null)
        {
            comparer ??= Comparer<T>.Default;
            int low = 0, high = Count - 1, result = -1;

            while (low <= high)
            {
                int mid = low + (high - low) / 2;
                int comparison = comparer.Compare(_array[mid], item);

                if (comparison < 0)
                {
                    low = mid + 1;
                }
                else if (comparison > 0)
                {
                    high = mid - 1;
                }
                else
                {
                    result = mid; 
                    high = mid - 1;
                }
            }

            return result;
        }

        public int BinarySearchLast(T item, IComparer<T> comparer = null)
        {
            comparer ??= Comparer<T>.Default;
            int low = 0, high = Count - 1, result = -1;

            while (low <= high)
            {
                int mid = low + (high - low) / 2;
                int comparison = comparer.Compare(_array[mid], item);

                if (comparison < 0)
                {
                    low = mid + 1;
                }
                else if (comparison > 0)
                {
                    high = mid - 1;
                }
                else
                {
                    result = mid;
                    low = mid + 1; 
                }
            }

            return result;
        }

        public T[] ToArray()
        {
            T[] array = new T[Count];
            for (int i = 0; i < Count; i++)
            {
                array[i] = _array[i];
            }
            return array;
        }
    }


}
