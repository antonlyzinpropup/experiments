using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Threading;

namespace BusinessRules.Tests
{
    [TestClass]
    public class IHateAsyncAwaitTests
    {
        [TestMethod]
        public void AsyncVsRegularCallTest()
        {
            //A test to compare a generic code with async/await VS proper task usage, vs sync mode
            //The test is measuring 100k requests like async Controller::method -> calls async "Service"::method -> calls few other "async" and await some long-running operations (like db, IO etc)
            var sw = new Stopwatch();

            sw.Start();
            Parallel.For(0, 100 * 1000, (i) =>
            {
                Task.WaitAll(SimulateSomeWorkAndCallItSelfAsync(150, 10));
            });

            Console.Out.WriteLine("Execution time async: " + sw.Elapsed.TotalSeconds); sw.Start();

            sw.Restart();

            Parallel.For(0, 100 * 1000, (i) =>
            {
                Task.WaitAll(SimulateSomeWorkAndCallItSelfAltAsync(150, 10));
            });

            Console.Out.WriteLine("Execution time async2: " + sw.Elapsed.TotalSeconds); sw.Start();

            sw.Restart();

            Parallel.For(0, 100 * 1000, (i) =>
            {
                SimulateSomeWorkAndCallItSelf(150, 10);
            });

            Console.Out.WriteLine("Execution time sync: " + sw.Elapsed.TotalSeconds);

            //Execution time async: 1124s
            //Execution time async2: 567s
            //Execution time sync: 105s
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
    }
}
