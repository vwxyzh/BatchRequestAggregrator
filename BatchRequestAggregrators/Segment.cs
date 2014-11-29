using System;
using System.Collections;
using System.Collections.Generic;

namespace Zhenway.BatchRequestAggregrators
{
    internal sealed class Segment<T> : IReadOnlyList<T>
    {
        private readonly IReadOnlyList<T> _list;

        private readonly int _startIndex;

        private readonly int _count;

        public Segment(IReadOnlyList<T> list, int startIndex, int count)
        {
            _list = list;
            _startIndex = startIndex;
            _count = count;
        }

        public T this[int index]
        {
            get
            {
                if (index > _count || index < 0)
                {
                    throw new ArgumentOutOfRangeException("index");
                }
                return _list[_startIndex + index];
            }
        }

        public int Count
        {
            get
            {
                return _count;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
