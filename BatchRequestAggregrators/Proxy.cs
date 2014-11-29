using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zhenway.BatchRequestAggregrators
{
    public sealed class Proxy<TArg, TResult>
    {
        private readonly ConcurrentDictionary<ProxyItem<TArg, TResult>, object> _aggregatableQueue =
            new ConcurrentDictionary<ProxyItem<TArg, TResult>, object>();
        private readonly Func<IReadOnlyList<TArg>, Task<IReadOnlyList<TResult>>> _func;
        private readonly IAggregateRule<TArg>[] _rules;
        private readonly Pool _pool;

        internal Proxy(Func<IReadOnlyList<TArg>, Task<IReadOnlyList<TResult>>> func, IAggregateRule<TArg>[] rules, Pool pool)
        {
            _func = func;
            _rules = rules;
            _pool = pool;
        }

        /// <summary>
        /// Invoke underlying batch operation with aggregating.
        /// </summary>
        public async Task<IReadOnlyList<TResult>> InvokeAsync(IReadOnlyList<TArg> args)
        {
            ProxyItem<TArg, TResult> proxyItem;
            int startIndex;
            if (!TryAggregate(args, out proxyItem, out startIndex))
            {
                proxyItem = new ProxyItem<TArg, TResult>(args);
                _aggregatableQueue.TryAdd(proxyItem, null);
                startIndex = 0;
                _pool.Queue(async () =>
                {
                    object _;
                    _aggregatableQueue.TryRemove(proxyItem, out _);
                    proxyItem.Close();
                    try
                    {
                        proxyItem.TaskCompletionSource.SetResult(await _func(proxyItem.Args));
                    }
                    catch (Exception ex)
                    {
                        proxyItem.TaskCompletionSource.SetException(ex);
                    }
                });
            }
            return (IReadOnlyList<TResult>)new Segment<TResult>(await proxyItem.TaskCompletionSource.Task, startIndex, args.Count);
        }

        private bool TryAggregate(
            IReadOnlyList<TArg> args,
            out ProxyItem<TArg, TResult> proxyItem,
            out int startIndex)
        {
            foreach (var item in _aggregatableQueue)
            {
                var current = item.Key;
                startIndex = current.TryAggregate(args, _rules);
                if (startIndex != -1)
                {
                    proxyItem = current;
                    return true;
                }
            }

            // unable to aggregate
            proxyItem = null;
            startIndex = -1;
            return false;
        }
    }
}
