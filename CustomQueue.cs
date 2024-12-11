using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KursovaSAAConsole2
{
    internal class CustomQueue<T> : IEnumerable<T>
    {
        private T[] _array;
        private int _start;
        private int _end;
        private int _count;
        private int _capacity;

        public int Count => _count;
        public int Capacity => _capacity;

        public CustomQueue(int initCapacity = 4)
        {
            _capacity = initCapacity;
            _array = new T[_capacity];
            _start = 0;
            _end = 0;
            _count = 0;
        }

        private void EnsureCapacity()
        {
            if (_count < _capacity) return;

            var newCapacity = _capacity * 2;
            var newArray = new T[newCapacity];

            if (_start <= _end)
            {
                Array.Copy(_array, _start, newArray, 0, _count);
            }
            else
            {
                Array.Copy(_array, _start, newArray, 0, _capacity - _start);
                Array.Copy(_array, 0, newArray, _capacity - _start, _end);
            }

            _array = newArray;
            _start = 0;
            _end = _count;
            _capacity = newCapacity;
        }

        public void Enqueue(T item)
        {
            EnsureCapacity();
            _array[_end] = item;
            _end = (_end + 1) % _capacity;
            _count++;
        }

        public T Dequeue()
        {
            if(_count == 0)
            {
                throw new InvalidOperationException("Queue is empty.");
            }

            var item = _array[_start];
            _array[_start] = default;
            _start = (_start + 1) % _capacity;
            _count--;


            return item;
        }

        public T Peek()
        {
            if (_count == 0)
            {
                throw new InvalidOperationException("Queue is empty.");
            }

            return _array[_start];
        }

        public bool Contains(T item)
        {
            var comparer = EqualityComparer<T>.Default;

            foreach(var element in this)
            {
                if(comparer.Equals(element, item))
                {
                    return true;
                }
            }
            return false;
        }

        public void Clear()
        {
            Array.Clear(_array, 0, _capacity);
            _start = 0;
            _end = 0;
            _count = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0, index = _start; i < _count; i++, index = (index + 1) % _capacity)
            {
                yield return _array[index];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
