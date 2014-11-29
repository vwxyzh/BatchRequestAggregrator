using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zhenway.BatchRequestAggregrators;

namespace UnitTest
{
    [TestClass]
    public class BatchRequestAggregatorTest
    {
        [TestMethod]
        public async Task TestBatchRequestAggregator()
        {
            var btp = new BatchRequestAggregator(4);
            var sw = Stopwatch.StartNew();
            var bag = new ConcurrentBag<int>();
            var proxy = btp.GetBuilder<int, string>(async xs =>
            {
                await Task.Delay(100);
                Console.WriteLine("batch size: {0}, @{1}, by thread:{2}", xs.Count, sw.ElapsedMilliseconds, Thread.CurrentThread.ManagedThreadId);
                bag.Add(xs.Count);
                return (from x in xs select x.ToString()).ToList();
            }).WithMaxBatchSize(50).Create();
            var tasks = (from t in Enumerable.Range(0, 100)
                         select proxy.InvokeAsync(Enumerable.Range(t, 10).ToList())).ToArray();
            await Task.WhenAll(tasks);
            Console.WriteLine("Total: {0}ms", sw.ElapsedMilliseconds);
            Console.WriteLine("Average size: {0}", bag.Average());
            Assert.IsTrue(sw.ElapsedMilliseconds > 200, "Pool not effective!");
            Assert.IsTrue(sw.ElapsedMilliseconds < 1000, "Merge not effective!");
            Assert.IsTrue(bag.Average() > 30.0, "Merge not effective!");
            for (int i = 0; i < tasks.Length; i++)
            {
                var results = tasks[i].Result;
                for (int j = 0; j < 10; j++)
                {
                    Assert.AreEqual((i + j).ToString(), results[j], "Error at {0}-{1}, Expected:{2}, Actual:{3}", i, j, (i + j).ToString(), results[j]);
                }
            }
        }
    }
}
