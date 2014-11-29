using System;
using System.Collections.Generic;

namespace Zhenway.BatchRequestAggregrators
{
    internal sealed class BatchSizeAggregateRule<T> : IAggregateRule<T>
    {
        private readonly int _maxBatchSize;

        public BatchSizeAggregateRule(int maxBatchSize)
        {
            _maxBatchSize = maxBatchSize;
        }

        public bool CanAggregate(IReadOnlyList<T> group, IReadOnlyList<T> another)
        {
            return group.Count + another.Count <= _maxBatchSize;
        }
    }
}
