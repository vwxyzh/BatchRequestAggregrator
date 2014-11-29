using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zhenway.BatchRequestAggregrators
{
    public sealed class ProxyBuilder<TArg, TResult>
    {
        private readonly Func<IReadOnlyList<TArg>, Task<IReadOnlyList<TResult>>> _func;
        private readonly Pool _pool;
        private readonly List<IAggregateRule<TArg>> _rules = new List<IAggregateRule<TArg>>();

        internal ProxyBuilder(Func<IReadOnlyList<TArg>, Task<IReadOnlyList<TResult>>> func, Pool pool)
        {
            _func = func;
            _pool = pool;
        }

        public ProxyBuilder<TArg, TResult> WithMaxBatchSize(int maxBatchSize)
        {
            if (maxBatchSize <= 0)
            {
                throw new ArgumentOutOfRangeException("maxBatchSize", "maxBatchSize must be greater than 0");
            }
            _rules.Add(new BatchSizeAggregateRule<TArg>(maxBatchSize));
            return this;
        }

        public ProxyBuilder<TArg, TResult> WithRule(Func<IReadOnlyList<TArg>, IReadOnlyList<TArg>, bool> func)
        {
            if (func == null)
            {
                throw new ArgumentNullException("func");
            }
            _rules.Add(new DelegatingAggregateRule<TArg>(func));
            return this;
        }

        public ProxyBuilder<TArg, TResult> WithRule(IAggregateRule<TArg> rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException("rule");
            }
            _rules.Add(rule);
            return this;
        }

        public Proxy<TArg, TResult> Create()
        {
            return new Proxy<TArg, TResult>(_func, _rules.ToArray(), _pool);
        }
    }
}
