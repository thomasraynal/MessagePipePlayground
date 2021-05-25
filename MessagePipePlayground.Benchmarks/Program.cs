using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using MessagePipePlayground.Sandbox;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MessagePipePlayground.Benchmarks
{
    internal class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            AddDiagnoser(MemoryDiagnoser.Default);
            AddJob(Job.ShortRun.WithWarmupCount(1).WithIterationCount(1).WithRuntime(CoreRuntime.Core31));
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            await MeasureEnqueueDequeueAllAsync(500);
            await MeasureEnqueueDequeueAllAsync(5000);
            await MeasureEnqueueDequeueAllAsync(50000);
            await MeasureEnqueueDequeueAllAsync(500000);
        }

        public static async Task MeasureEnqueueDequeueAllAsync(int nbItems)
        {
            var emptyQueue = new Queue(nbItems);
            var emptyFastQueue = new FastQueue<long>(nbItems);

            (string, TimeSpan)[] result = new (string, TimeSpan)[0];
            for (int i = 0; i < 2; i++)
            {
                if (i == 0) Console.WriteLine("WARM:");
                if (i == 1) Console.WriteLine("RUN:");

                var index = 0L;

                result = new (string, TimeSpan)[]
                {

                      Measure("Queue - Enqueue/Dequeue", () => 
                      {
                          for(var i = 0; i< nbItems; i++)
                          {
                             emptyQueue.Enqueue(index++);
                          }

                             for(var i = 0; i< nbItems; i++)
                          {
                             emptyQueue.Dequeue();
                          }
                       }),
                      Measure("FastQueue - Enqueue/Dequeue", () =>
                      {
                          for(var i = 0; i< nbItems; i++)
                          {
                             emptyFastQueue.Enqueue(index++);
                          }

                             for(var i = 0; i< nbItems; i++)
                          {
                             emptyFastQueue.Dequeue();
                          }
                       }),

                };
            }
         
            Console.WriteLine("----");
            Console.WriteLine();

            foreach (var item in result.OrderByDescending(x => x.Item2))
            {
                Console.WriteLine($"{item.Item1} - {nbItems} - {item.Item2.TotalMilliseconds} ms");
            }

            Console.WriteLine();
        }


        static (string, TimeSpan) Measure(string label, Action action)
        {
            Console.WriteLine("Start:" + label);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            GC.TryStartNoGCRegion(1000 * 1000 * 100, true);

       
            var sw = Stopwatch.StartNew();

            action();

            sw.Stop();

            try
            {
                GC.EndNoGCRegion();
            }
            catch
            {
                Console.WriteLine("Faile NoGC:" + label);
            }

            return (label, sw.Elapsed);
        }
    }

    
}
