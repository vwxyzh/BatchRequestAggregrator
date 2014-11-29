using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Zhenway.BatchRequestAggregrators
{
    internal sealed class ProxyItem<TArg, TResult>
    {
        private bool _isClosed;

        public ProxyItem(IReadOnlyList<TArg> args)
        {
            Args = args.ToList();
            TaskCompletionSource = new TaskCompletionSource<IReadOnlyList<TResult>>();
        }

        public List<TArg> Args { get; private set; }

        public TaskCompletionSource<IReadOnlyList<TResult>> TaskCompletionSource { get; private set; }

        public void Close()
        {
            lock (this)
            {
                _isClosed = true;
            }
        }

        public int TryAggregate(IReadOnlyList<TArg> args, IAggregateRule<TArg>[] rules)
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(this, ref lockTaken);
                if (lockTaken)
                {
                    if (_isClosed)
                    {
                        return -1;
                    }
                    if (rules.All(r => r.CanAggregate(Args, args)))
                    {
                        var startIndex = Args.Count;
                        Args.AddRange(args);
                        return startIndex;
                    }
                }
                return -1;
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(this);
                }
            }
        }
    }
}
