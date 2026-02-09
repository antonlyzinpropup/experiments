using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Threading;
using Nito.AsyncEx.Synchronous;

namespace BusinessRules.Tests
{
    [TestClass]
    public class IHateAsyncAwaitTests
    {
        [TestMethod]
        public void AsyncVsRegularSyntheticCallTest()
        {
            var sw = new Stopwatch();

            sw.Start();
            Parallel.For(0, 100 * 1000, new ParallelOptions(){MaxDegreeOfParallelism = 100 * 1000}, (i) =>
            {
                SimulateSomeWorkAndCallItSelfAsync(150, 10).WaitAndUnwrapException();
            });

            // 1124s
            Console.Out.WriteLine("Execution time async: " + sw.Elapsed.TotalSeconds); sw.Start();
            
            
            sw.Restart();

            Parallel.For(0, 100 * 1000, (i) =>
            {
                SimulateSomeWorkAndCallItSelfAltAsync(150, 10).Wait();
            });

            //567s
            Console.Out.WriteLine("Execution time async2: " + sw.Elapsed.TotalSeconds); sw.Start();
            
            
            sw.Restart();
            Task.WaitAll(Enumerable.Range(0, 100 * 1000).Select(i => SimulateSomeWorkAndCallItSelfAsync(150, 10)).ToArray());

            //2.5s
            Console.Out.WriteLine("Execution time async (method 2): " + sw.Elapsed.TotalSeconds);
            

            sw.Restart();
            Task.WaitAll(Enumerable.Range(0, 100 * 1000).Select(i => new Task(() => SimulateSomeWorkAndCallItSelf(150, 10))).ToArray());

            Console.Out.WriteLine("Execution time sync (method 2): " + sw.Elapsed.TotalSeconds);
            
            sw.Restart();

            Parallel.For(0, 100 * 1000, (i) =>
            {
                SimulateSomeWorkAndCallItSelf(150, 10);
            });

            //105s
            Console.Out.WriteLine("Execution time sync: " + sw.Elapsed.TotalSeconds);
        }

        [TestMethod]
        public void AsyncVsRegularRealWorkCallTest()
        {
            var sw = new Stopwatch();

            sw.Start();
            Parallel.For(0, 100 * 1000, new ParallelOptions(){MaxDegreeOfParallelism = 100 * 1000}, (i) =>
            {
                SimulateSomeRealWorkAndCallItSelfAsync(150, 10).WaitAndUnwrapException();
            });

            // 985s
            Console.Out.WriteLine("Execution time async: " + sw.Elapsed.TotalSeconds); sw.Start();
            
            sw.Restart();

            Parallel.For(0, 100 * 1000, (i) =>
            {
                SimulateSomeRealWorkAndCallItSelfAltAsync(150, 10).Wait();
            });

            //1616s
            Console.Out.WriteLine("Execution time async2: " + sw.Elapsed.TotalSeconds); sw.Start();
            
            
            sw.Restart();
            Task.WaitAll(Enumerable.Range(0, 100 * 1000).Select(i => SimulateSomeRealWorkAndCallItSelfAsync(150, 10)).ToArray<Task>());

            //.NET process consumed over 20gb on 60kb file, test never complete
            Console.Out.WriteLine("Execution time async (method 2): " + sw.Elapsed.TotalSeconds);
            

            sw.Restart();

            Parallel.For(0, 100 * 1000, (i) =>
            {
                SimulateSomeRealWorkAndCallItSelf(150, 10);
            });

            //409s
            Console.Out.WriteLine("Execution time sync: " + sw.Elapsed.TotalSeconds);
        }

        private static async Task SimulateSomeWorkAndCallItSelfAsync(int msDelay, int moreDeepCalls)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(msDelay));

            if (moreDeepCalls > 0)
            {
                await SimulateSomeWorkAndCallItSelfAsync(msDelay, moreDeepCalls - 1);
            }
        }

        private static Task SimulateSomeWorkAndCallItSelfAltAsync(int msDelay, int moreDeepCalls)
        {
            if (moreDeepCalls > 0)
            {
                Task.Delay(TimeSpan.FromMilliseconds(msDelay)).Wait();

                return SimulateSomeWorkAndCallItSelfAltAsync(msDelay, moreDeepCalls - 1);
            }

            return Task.Delay(TimeSpan.FromMilliseconds(msDelay));
        }

        private static void SimulateSomeWorkAndCallItSelf(int msDelay, int moreDeepCalls)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(msDelay));

            if (moreDeepCalls > 0)
            {
                SimulateSomeWorkAndCallItSelf(msDelay, moreDeepCalls - 1);
            }
        }

        private static async Task<string[]> SimulateSomeRealWorkAndCallItSelfAsync(int msDelay, int moreDeepCalls)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(msDelay));

            if (moreDeepCalls > 0)
            {
                return await SimulateSomeRealWorkAndCallItSelfAsync(msDelay, moreDeepCalls - 1);
            }

            //Read file...
            return await File.ReadAllLinesAsync(@"C:\\temp\\allMetrics.csv");
        }

        private static Task<string[]> SimulateSomeRealWorkAndCallItSelfAltAsync(int msDelay, int moreDeepCalls)
        {
            if (moreDeepCalls > 0)
            {
                Task.Delay(TimeSpan.FromMilliseconds(msDelay)).Wait();

                return SimulateSomeRealWorkAndCallItSelfAltAsync(msDelay, moreDeepCalls - 1);
            }
            
            //Read file...
            return File.ReadAllLinesAsync(@"C:\\temp\\allMetrics.csv");
        }

        private static string[] SimulateSomeRealWorkAndCallItSelf(int msDelay, int moreDeepCalls)
        {
            Thread.Sleep(TimeSpan.FromMilliseconds(msDelay));

            if (moreDeepCalls > 0)
            {
                return SimulateSomeRealWorkAndCallItSelf(msDelay, moreDeepCalls - 1);
            }
            
            //Read file...
            return File.ReadAllLines(@"C:\\temp\\allMetrics.csv");
        }
    }
}
