using System;
using System.Collections.Generic;

namespace Zhenway.BatchRequestAggregrators
{
    public interface IAggregateRule<T>
    {
        bool CanAggregate(IReadOnlyList<T> group, IReadOnlyList<T> another);
    }
}
