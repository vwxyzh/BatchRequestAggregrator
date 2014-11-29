using System;
using System.Collections.Generic;

namespace Zhenway.BatchRequestAggregrators
{
    internal sealed class DelegatingAggregateRule<T> : IAggregateRule<T>
    {
        private readonly Func<IReadOnlyList<T>, IReadOnlyList<T>, bool> _func;

        public DelegatingAggregateRule(Func<IReadOnlyList<T>, IReadOnlyList<T>, bool> func)
        {
            _func = func;
        }

        public bool CanAggregate(IReadOnlyList<T> group, IReadOnlyList<T> another)
        {
            return _func(group, another);
        }
    }
}
