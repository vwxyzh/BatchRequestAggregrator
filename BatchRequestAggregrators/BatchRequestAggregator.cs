using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zhenway.BatchRequestAggregrators
{
    public class BatchRequestAggregator
    {
        private readonly Pool _pool;

        public BatchRequestAggregator(int maxConcurrency)
        {
            _pool = new Pool(maxConcurrency);
        }

        /// <summary>
        /// get a proxy builder
        /// </summary>
        /// <typeparam name="TArg">the type of argument element</typeparam>
        /// <typeparam name="TResult">the type of result element</typeparam>
        /// <param name="func">the raw batch func, it should keep args -> result in order and no skip.</param>
        /// <returns>an instance of proxy builder</returns>
        public ProxyBuilder<TArg, TResult> GetBuilder<TArg, TResult>(Func<IReadOnlyList<TArg>, Task<IReadOnlyList<TResult>>> func)
        {
            return new ProxyBuilder<TArg, TResult>(func, _pool);
        }
    }
}
