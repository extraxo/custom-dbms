using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    public class CustomDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly EqualityComparer<TKey> _equalityComparer = EqualityComparer<TKey>.Default;
        private CustomList<KeyValuePair<TKey, TValue>>[] _items;
        private const int defaultCapacity = 16;
        public int Count { get; private set; }

        public CustomDictionary(int capacity = defaultCapacity)
        {
            _items = new CustomList<KeyValuePair<TKey, TValue>>[capacity];
        }

        public TValue this[TKey key]
        {
            get
            {
                var hash = _equalityComparer.GetHashCode(key);
                hash = Math.Abs(hash);
                hash %= _items.Length;

                var list = _items[hash];

                if (list == null || list.Count == 0)
                {
                    throw new KeyNotFoundException();
                }

                foreach (var item in list)
                {
                    if (_equalityComparer.Equals(item.Key, key))
                        return item.Value;
                }

                throw new KeyNotFoundException();
            }

            set
            {
                Add(key, value);
            }
        }

        public void Add(TKey key, TValue value)
        {
            var hash = _equalityComparer.GetHashCode(key);
            hash = Math.Abs(hash);
            hash %= _items.Length;

            var list = _items[hash];

            if (list == null)
            {
                list = new CustomList<KeyValuePair<TKey, TValue>>();
                list = [];
                _items[hash] = list;
            }
            else
            {
                foreach (var item in list)
                {
                    if (_equalityComparer.Equals(item.Key, key))
                    {
                        throw new KeyNotFoundException();
                    }
                }
            }

            list.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public bool ContainsKey(TKey key)
        {
            var hash = _equalityComparer.GetHashCode(key);
            hash = Math.Abs(hash);
            hash %= _items.Length;
            var list = _items[hash];
            if (list == null)
            {
                return false;
            }
            foreach (var item in list)
            {
                if (_equalityComparer.Equals(item.Key, key))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Remove(TKey key)
        {
            var hash = _equalityComparer.GetHashCode(key);
            hash = Math.Abs(hash);
            hash %= _items.Length;
            var list = _items[hash];

            if (list == null)
            {
                return false;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (_equalityComparer.Equals(list[i].Key, key))
                {
                    list.RemoveAt(i);
                    Count--;
                    return true;
                }
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            var hash = _equalityComparer.GetHashCode(key);
            hash = Math.Abs(hash);
            hash %= _items.Length;
            var list = _items[hash];

            if (list != null)
            {
                foreach (var item in list)
                {
                    if (_equalityComparer.Equals(item.Key, key))
                    {
                        value = item.Value;
                        return true;
                    }
                }
            }
            value = default;
            return false;
        }

        public void Clear()
        {
            _items = new CustomList<KeyValuePair<TKey, TValue>>[defaultCapacity];
            Count = 0;
        }

        public ICollection<TKey> Keys
        {
            get
            {
                var keys = new CustomList<TKey>();
                foreach (var list in _items)
                {
                    if (list != null)
                    {
                        foreach (var item in list)
                        {
                            keys.Add(item.Key);
                        }
                    }
                }
                return keys;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var list in _items)
            {
                if (list != null)
                {
                    foreach (var item in list)
                    {
                        yield return item;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool IsReadOnly => false;
        public ICollection<TValue> Values
        {
            get
            {
                var values = new CustomList<TValue>();
                foreach (var list in _items)
                {
                    if (list != null)
                    {
                        foreach (var item in list)
                        {
                            values.Add(item.Value);
                        }

                    }
                }
                return values;
            }
        }
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0 || arrayIndex + Count > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            foreach (var list in _items)
            {
                if (list != null)
                {
                    foreach (var item in list)
                    {
                        array[arrayIndex++] = item;
                    }
                }
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Contains(item) && Remove(item.Key);
        }

    }
}
